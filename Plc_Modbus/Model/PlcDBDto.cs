using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Model
{
    public class PlcDBDto
    {
        public string Equip_id { get; set; }
        public string Address { get; set; }
        public string Metric_name { get; set; }
        public double Metric_value { get; set; }
    }
}
