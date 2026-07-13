using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plc_Modbus.data
{
    public class Plc_Writer
    {
        private readonly Mod_Conn _Conn;

        public Plc_Writer(Mod_Conn Conn)
        {
            _Conn = Conn;
        }

        public async Task writeData(bool[] writeCoil)
        {
            try
            {
                if (_Conn.modbusMaster == null) return;
                await _Conn._plcLock.WaitAsync();
                try
                {
                    await _Conn.modbusMaster.WriteMultipleCoilsAsync(1, 0, writeCoil);
                }
                finally
                {
                    _Conn._plcLock.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"오류발생: {ex.Message}");
            }
        }

    }
}
