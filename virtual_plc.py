import argparse
import asyncio
import random
import time

from pymodbus.datastore import (
    ModbusSequentialDataBlock,
    ModbusServerContext,
    ModbusSlaveContext,
)
from pymodbus.server import StartAsyncTcpServer


# Holding Register map (display address -> zero-based offset)
PRODUCTION_COUNT = 0      # 40001, EA
EQUIPMENT_STATUS = 1      # 40002, 0=STOP, 1=RUN, 2=ERROR, 3=COMPLETE
CYCLE_TIME = 2            # 40003, raw / 100 = seconds
ALARM_CODE = 3            # 40004, 0=normal, see ALARM_* below
VISION_RESULT = 4         # 40005, 0=not inspected, 1=OK, 2=NG
VISION_EVENT_SEQUENCE = 5 # 40006, increments on every inspection
TARGET_QUANTITY = 6       # 40007, EA, written by MES when a work order starts
TEMPERATURE = 9           # 40010, raw / 10 = degrees Celsius
PRESSURE = 10             # 40011, raw / 100 = bar
CURRENT = 11              # 40012, raw / 10 = A
INSTANT_POWER = 12        # 40013, raw / 100 = kW
ENERGY_HIGH_WORD = 13     # 40014, accumulated energy high word
ENERGY_LOW_WORD = 14      # 40015, accumulated energy low word, raw / 100 = kWh

STOP = 0
RUN = 1
ERROR = 2
COMPLETE = 3

NOT_INSPECTED = 0
OK = 1
NG = 2

NO_ALARM = 0
ALARM_E_STOP = 1001            # 비상정지
ALARM_SENSOR_FAULT = 1002      # 센서 이상
ALARM_MATERIAL_SHORTAGE = 1003 # 자재 부족
FORCED_ERROR_ALARM_CODES = [
    ALARM_E_STOP,
    ALARM_SENSOR_FAULT,
    ALARM_MATERIAL_SHORTAGE,
]

REGISTER_COUNT = 20
POLL_INTERVAL_SECONDS = 0.25
METRIC_INTERVAL_SECONDS = 1.0
PRODUCTION_INTERVAL_SECONDS = 2.0


# Coil 00001(offset 0): RUN command, Coil 00002(offset 1): force ERROR.
# Input Register 30001~30002 mirrors temperature/pressure for the existing
# WinForms collector while it is migrated to the guide's Holding Register map.
store = ModbusSlaveContext(
    co=ModbusSequentialDataBlock(0, [0] * 10),
    di=ModbusSequentialDataBlock(0, [0] * 10),
    hr=ModbusSequentialDataBlock(0, [0] * REGISTER_COUNT),
    ir=ModbusSequentialDataBlock(0, [0] * REGISTER_COUNT),
    zero_mode=True,
)
context = ModbusServerContext(slaves=store, single=True)


def clamp(value: float, minimum: float, maximum: float) -> float:
    return max(minimum, min(value, maximum))


def split_uint32(value: int) -> tuple[int, int]:
    """Return a 32-bit value in high-word, low-word order."""
    value &= 0xFFFFFFFF
    return (value >> 16) & 0xFFFF, value & 0xFFFF


def set_holding(offset: int, value: int) -> None:
    context[0].store["h"].setValues(offset, [value & 0xFFFF])


def get_holding(offset: int) -> int:
    return context[0].store["h"].getValues(offset, 1)[0]


def get_coil(offset: int) -> bool:
    return bool(context[0].store["c"].getValues(offset, 1)[0])


def set_sensor_registers(
    temperature: float,
    pressure: float,
    current: float,
    power: float,
    accumulated_energy: float,
) -> None:
    temperature_raw = int(round(temperature * 10))
    pressure_raw = int(round(pressure * 100))
    current_raw = int(round(current * 10))
    power_raw = int(round(power * 100))
    energy_raw = int(round(accumulated_energy * 100))
    energy_high, energy_low = split_uint32(energy_raw)

    holding = context[0].store["h"]
    holding.setValues(TEMPERATURE, [temperature_raw])
    holding.setValues(PRESSURE, [pressure_raw])
    holding.setValues(CURRENT, [current_raw])
    holding.setValues(INSTANT_POWER, [power_raw])
    holding.setValues(ENERGY_HIGH_WORD, [energy_high, energy_low])

    # Temporary compatibility mirror for the current WinForms implementation.
    input_registers = context[0].store["i"]
    input_registers.setValues(0, [temperature_raw])
    input_registers.setValues(1, [int(round(pressure * 10))])


async def generate_factory_data() -> None:
    production_count = 0
    vision_sequence = 0
    accumulated_energy = 0.0
    temperature = 24.0
    pressure = 0.0
    current = 0.0
    power = 0.0

    previous_status = STOP
    previous_target_quantity = 0
    previous_force_error = False
    current_alarm_code = NO_ALARM
    completed_latch = False
    last_metric_at = time.monotonic()
    last_production_at = time.monotonic()
    last_loop_at = time.monotonic()

    set_holding(PRODUCTION_COUNT, production_count)
    set_holding(EQUIPMENT_STATUS, STOP)
    set_holding(CYCLE_TIME, 0)
    set_holding(ALARM_CODE, NO_ALARM)
    set_holding(VISION_RESULT, NOT_INSPECTED)
    set_holding(VISION_EVENT_SEQUENCE, vision_sequence)
    set_holding(TARGET_QUANTITY, 0)
    set_sensor_registers(
        temperature, pressure, current, power, accumulated_energy
    )

    while True:
        now = time.monotonic()
        elapsed_seconds = now - last_loop_at
        last_loop_at = now

        run_command = get_coil(0)
        force_error = get_coil(1)

        # A new target quantity means a new work order was handed to the PLC.
        # Reset the counter and any latched completion, but only while the
        # equipment is not actively running an existing job.
        target_quantity = get_holding(TARGET_QUANTITY)
        if target_quantity != previous_target_quantity:
            if previous_status != RUN:
                production_count = 0
                completed_latch = False
            previous_target_quantity = target_quantity

        # Pick a fresh alarm reason each time ERROR is newly forced, so
        # downstream equip_downtime/common_code data has variety.
        if force_error and not previous_force_error:
            current_alarm_code = random.choice(FORCED_ERROR_ALARM_CODES)
        elif not force_error:
            current_alarm_code = NO_ALARM
        previous_force_error = force_error

        if force_error:
            status = ERROR
        elif completed_latch:
            status = COMPLETE
            if not run_command:
                # MES acknowledged completion by dropping RUN; go idle.
                completed_latch = False
                status = STOP
        elif run_command:
            status = RUN
        else:
            status = STOP

        if status != previous_status:
            if status == RUN:
                # A new production interval begins when the equipment starts.
                last_production_at = now
            elif status in (STOP, COMPLETE):
                set_holding(CYCLE_TIME, 0)
            previous_status = status

        set_holding(EQUIPMENT_STATUS, status)
        # These registers are PLC outputs. Restore the internal counter if an
        # older test client writes to Holding Register 40001.
        set_holding(PRODUCTION_COUNT, production_count)
        set_holding(ALARM_CODE, current_alarm_code)

        if status == RUN:
            # Energy integration: kW * hours = kWh.
            accumulated_energy += power * elapsed_seconds / 3600.0

            if now - last_metric_at >= METRIC_INTERVAL_SECONDS:
                temperature = clamp(temperature + random.uniform(-0.5, 0.8), 25.0, 80.0)
                pressure = clamp(pressure + random.uniform(-0.08, 0.08), 2.5, 4.5)
                current = clamp(current + random.uniform(-0.3, 0.3), 4.0, 8.0)
                power = clamp(current * random.uniform(0.38, 0.45), 1.5, 4.0)
                last_metric_at = now

            if now - last_production_at >= PRODUCTION_INTERVAL_SECONDS:
                cycle_seconds = now - last_production_at
                last_production_at = now

                production_count = (production_count + 1) & 0xFFFF
                vision_sequence = (vision_sequence + 1) & 0xFFFF
                vision_result = NG if random.random() < 0.05 else OK

                # Make an occasional pressure spike visible around an NG event.
                if vision_result == NG:
                    pressure = random.uniform(5.5, 7.0)

                set_holding(PRODUCTION_COUNT, production_count)
                set_holding(CYCLE_TIME, int(round(cycle_seconds * 100)))
                set_holding(VISION_RESULT, vision_result)
                set_holding(VISION_EVENT_SEQUENCE, vision_sequence)

                if target_quantity > 0 and production_count >= target_quantity:
                    completed_latch = True

        elif status == STOP:
            temperature = clamp(temperature + random.uniform(-0.2, 0.1), 22.0, 30.0)
            pressure = 0.0
            current = 0.0
            power = 0.0
            last_metric_at = now

        elif status == COMPLETE:
            temperature = clamp(temperature + random.uniform(-0.3, 0.1), 22.0, 30.0)
            pressure = 0.0
            current = 0.0
            power = 0.0
            last_metric_at = now

        else:  # ERROR
            pressure = 0.0
            current = 0.0
            power = 0.0
            last_metric_at = now

        set_sensor_registers(
            temperature, pressure, current, power, accumulated_energy
        )

        if int(now) != int(now - elapsed_seconds):
            vision_raw = context[0].store["h"].getValues(VISION_RESULT, 1)[0]
            vision_text = {NOT_INSPECTED: "미검사", OK: "OK", NG: "NG"}.get(
                vision_raw, "UNKNOWN"
            )
            status_text = {
                STOP: "STOP",
                RUN: "RUN",
                ERROR: "ERROR",
                COMPLETE: "COMPLETE",
            }[status]
            print(
                f"[PLC] 상태={status_text:<8} 목표={target_quantity:>5} EA "
                f"생산={production_count:>5} EA "
                f"검사={vision_text:<3} Seq={vision_sequence:>5} "
                f"온도={temperature:>4.1f} ℃ 압력={pressure:>4.2f} bar "
                f"전류={current:>4.1f} A 전력={power:>4.2f} kW"
            )

        await asyncio.sleep(POLL_INTERVAL_SECONDS)


async def main(host: str, port: int) -> None:
    print(f"가상 PLC 서버 시작 - {host}:{port}")
    print("Coil 00001: RUN/STOP, Coil 00002: 강제 ERROR")
    print("Holding Register 40007: 목표수량(TargetQuantity) - MES가 작업 시작 시 기록")
    await asyncio.gather(
        StartAsyncTcpServer(context=context, address=(host, port)),
        generate_factory_data(),
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Mini MES Modbus TCP Virtual PLC")
    parser.add_argument("--host", default="127.0.0.1", help="server bind address")
    parser.add_argument("--port", type=int, default=502, help="server TCP port")
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    try:
        asyncio.run(main(args.host, args.port))
    except KeyboardInterrupt:
        print("가상 PLC 서버를 종료합니다.")