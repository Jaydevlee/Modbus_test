using NModbus;
using Plc_Modbus.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Plc_Modbus.data
{
    public class Mod_Conn
    {
        public IModbusMaster? modbusMaster { get; private set; }
        public SemaphoreSlim _plcLock = new (1, 1);
        private TcpClient? _client;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public async Task<bool> PlcConnect()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 502);
                var factory = new ModbusFactory();
                modbusMaster = factory.CreateMaster(_client);
                if (modbusMaster == null)
                {
                    Debug.WriteLine("연결실패");
                    _client.Dispose();
                    await retry();
                    return false;
                }
                else
                {
                    Debug.WriteLine("연결 성공");
                    return true;
                }
            } 
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"오류 발생: {ex.Message}");
                return false;  
            }
        }
        // 폼 종료 시 연결 닫기
        public void Dispose()
        {
            _cts.Cancel();
            modbusMaster?.Dispose();
            _client?.Dispose();
            _plcLock.Dispose();

            modbusMaster = null;
            _client = null;
        }

        public async Task<bool> retry()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (await PlcConnect()) return true;
                await Task.Delay(2000, _cts.Token);
            }
            return false;
        }
    }
}
