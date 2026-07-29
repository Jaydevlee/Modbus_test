import asyncio # 비동기 처리 (기본내장)
import random # 랜덤값 생성 (기본내장)
from pymodbus.server import StartAsyncTcpServer 
from pymodbus.datastore import (
    ModbusSlaveContext, # plc 1대의 메모리 공간
    ModbusServerContext, # 여러 PLC를 묶는 컨테이너
    ModbusSequentialDataBlock # 실제 메모리 블록
)
from pymodbus.server import StartAsyncTcpServer # Modbus TCP를 여는 함수

store = ModbusSlaveContext(
    co=ModbusSequentialDataBlock(0, [0]*10),
    di=ModbusSequentialDataBlock(0, [0]*10),
    hr=ModbusSequentialDataBlock(0, [0]*10),
    ir=ModbusSequentialDataBlock(0, [0]*10),
    # zero_mode=False(기본값)면 pymodbus가 와이어 주소에 +1을 해서 저장소에 접근함
    # (예: 클라이언트가 주소 0에 쓰면 실제로는 raw 1번에 저장)
    # generate_sensor_data()는 store 블록에 오프셋 없이 직접 접근하므로 어긋남 -> True로 고정
    zero_mode=True
)

context = ModbusServerContext(slaves=store, single=True) # ModbusServerContext = 여러 PLC를 관리하는 컨테이너, single=True 일 때는 PLC가 1대

async def generate_sensor_data():
    while True:
        # 0번 슬레이브(PLC) 선택
        # single=True 면 항상 0번

        # 만약 PLC 여러 대라면
        # context[1] → 1번 PLC
        # context[2] → 2번 PLC
        holding_registers = context[0].store['h'] # Holding Register 메모리 블록만 꺼내오기
        input_registers = context[0].store['i']
        motor = context[0].store['c'].getValues(0, 1)[0];
        error = context[0].store['c'].getValues(1, 1)[0];
        speed = holding_registers.getValues(0, 1)[0] / 10

        if(motor == True and speed != 0.0):
            temperature = round(random.uniform(20.0, 80.0), 1)
            pressure = round(random.uniform(1.0, 5.0), 1)
            # modbus에 소숫점 값 저장 시
            # Modbus는 정수만 저장 가능하여 -> 10배 곱해서 저장
            # 예) 온도 78.5 -> 785 읽을 때 10으로 나눔
            input_registers.setValues(0, [int(temperature * 10)])
            input_registers.setValues(1, [int(pressure * 10)])
        else:
            temperature = 0
            pressure = 0
            speed = 0.0
            holding_registers.setValues(0, [0])
            input_registers.setValues(0, [int(temperature * 10)])
            input_registers.setValues(1, [int(pressure * 10)])

        if(error == True):
            temperature = 0
            pressure = 0
            speed = 0.0
            # float(0.0)을 넣으면 나중에 C#이 이 레지스터를 읽을 때
            # 서버가 16비트 정수로 인코딩하다 실패해서 응답을 못 주고 타임아웃남 -> 정수 리스트로 저장
            holding_registers.setValues(0, [0])
            motor = False
            context[0].store['c'].setValues(0, [False])

        print(f"[PLC] 온도: {temperature}°C | 압력: {pressure}bar | " 
            f" 속도: {speed}rpm | 모터: {'on' if motor else 'off'} | 에러: {'error' if error else 'none'}")
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