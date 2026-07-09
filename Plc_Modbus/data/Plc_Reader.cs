using Plc_Modbus.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plc_Modbus.data
{
    public class Plc_Reader
    {
        private readonly Mod_Conn _Conn;
        private readonly Action<PlcDto> _onDataGrid;
        public ushort[] readData { get; private set; } = new ushort[10];
        public PlcDto latesData { get; private set; } = new PlcDto();

        public Plc_Reader(Mod_Conn conn, Action<PlcDto> onDataGrid)
        {
            _Conn = conn;
            _onDataGrid = onDataGrid;
        }

        public async Task ReadPlc()
        {
            // 접속 여부 확인
            if (!_Conn.PlcConnect())
            {
                Debug.WriteLine("PLC 연결 실패");
                return; // 연결 실패 시 메서드 종료
            }

            Debug.WriteLine("PLC 연결 성공");

            // 데이터 읽기 시작
            while (true)
            {
                try
                {
                    await _Conn._plcLock.WaitAsync();
                    try
                    {
                        // 코일 영역(비트) 읽기
                        bool[] readCoil = await _Conn.modbusMaster.ReadCoilsAsync(1, 0, 2);
                        Debug.WriteLine($"coil0: {readCoil[0]}, coil1: {readCoil[1]}");

                        // 패킷 엉킴 방지용 미세 딜레이 
                        await Task.Delay(50);

                        // 홀딩 영역(워드) 읽기
                        ushort[] readHolding = await _Conn.modbusMaster.ReadHoldingRegistersAsync(1, 0, 3);
                        Debug.WriteLine($"holding0: {readHolding[0]}, holding1: {readHolding[1]}");

                        PlcDto currentData = new PlcDto
                        {
                            coil_val1 = readCoil[0],
                            coil_val2 = readCoil[1],
                            hold_val1 = readHolding[0],
                            hold_val2 = readHolding[1]
                        };

                        this.latesData = currentData;
                        _onDataGrid?.Invoke(currentData);
                    }
                    finally
                    {
                        _Conn._plcLock.Release();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[통신 루프 에러 발생]: {ex.Message}");
                }

                // 0.3초 대기 후 다시 읽기
                await Task.Delay(300);
            }
        }
    }
}
