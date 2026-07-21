import asyncio # 비동기 처리 (기본내장)
import random # 랜덤값 생성 (기본내장)
from pymodbus.server import StartAsyncTcpServer 
from pymodbus.datastore import (
    ModbusSlaveContext, # plc 1대의 메모리 공간
    ModbusServerContext, # 여러 PLC를 묶는 컨테이너
    ModbusSequentialDataBlock # 실제 메모리 블록
)
from pymodbus.server import StartAsyncTcpServer # Modbus TCP를 여는 함수

# modbus에 소숫점 값 저장 시
# Modbus는 정수만 저장 가능하여 -> 10배 곱해서 저장
# 예) 온도 78.5 -> 785 읽을 때 10으로 나눔

store = ModbusSlaveContext(
    co=ModbusSequentialDataBlock(0, [0]*10),
    di=ModbusSequentialDataBlock(0, [0]*10),
    hr=ModbusSequentialDataBlock(0, [0]*10),
    ir=ModbusSequentialDataBlock(0, [0]*10)
)

context = ModbusServerContext(slaves=store, single=True) # ModbusServerContext = 여러 PLC를 관리하는 컨테이너, single=True 일 때는 PLC가 1대

async def generate_sensor_data():
    while True:
        temperature = round(random.uniform(20.0, 80.0), 1)
        pressure = round(random.uniform(1.0, 5.0), 1)

        # 0번 슬레이브(PLC) 선택
        # single=True 면 항상 0번

        # 만약 PLC 여러 대라면
        # context[1] → 1번 PLC
        # context[2] → 2번 PLC
        holding_registers = context[0].store['h'] # Holding Register 메모리 블록만 꺼내오기
        input_registers = context[0].store['i']

        input_registers.setValues(1, [int(temperature * 10)])
        input_registers.setValues(2, [int(pressure * 10)])

        speed = holding_registers.getValues(3, 1)[0] / 10
        motor = context[0].store['c'].getValues(1, 1)[0]

        print(f"[PLC] 온도: {temperature}°C | 압력: {pressure}bar | " 
            f" 속도: {speed}rpm | 모터: {'on' if motor else 'off'}")
        await asyncio.sleep(1)
        
async def main():
    print("가상 PLC 서버 시작 - localhost:502")
    await asyncio.gather(
        StartAsyncTcpServer(context, address=("localhost", 502)),
        generate_sensor_data()
    )

# __name__: 현재 모듈의 이름을 담고 있는 내장 변수
# 모듈의 직접 실행 여부 판단
if __name__ == "__main__":
    asyncio.run(main())