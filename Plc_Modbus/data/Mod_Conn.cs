using NModbus;
using Plc_Modbus.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Plc_Modbus.data
{
    public class Mod_Conn
    {
        public IModbusMaster modbusMaster { get; private set; }
        public SemaphoreSlim _plcLock = new (1, 1);
        public bool PlcConnect()
        {
            TcpClient client = new TcpClient("127.0.0.1", 502);
            try
            {
                var factory = new ModbusFactory();
                modbusMaster = factory.CreateMaster(client);
                if (modbusMaster == null)
                {
                    Debug.WriteLine("연결실패");
                    return false;
                }
                else
                {
                    Debug.WriteLine("연결 성공");
                    return true;
                }
            } 
            catch (Exception ex) 
            {
                Debug.WriteLine($"오류 발생: {ex.Message}");
                return false;
            }
        }
    }
}
