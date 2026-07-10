using Dapper;
using Npgsql;
using Plc_Modbus.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Plc_Modbus.data
{
    public class PLC_Repository
    {
        private readonly DB_Conn _Conn = new DB_Conn();

        public void InsertSensorData(List<PlcDBDto> readData)
        {
            using (var conn = _Conn.CreateConn())
            {
                try
                {
                    conn.Open();
                    string sql = @"INSERT INTO sensor_data (time, equip_id, address, metric_name, metric_value)
                            VALUES(NOW(), @Equip_id, @Address, @Metric_name, @Metric_value)";
                    conn.Execute(sql, readData);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
