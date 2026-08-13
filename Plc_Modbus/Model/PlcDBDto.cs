using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Model
{
    public class PlcDBDto
    {
        public string Equip_id { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Metric_name { get; set; } = string.Empty;
        public double Metric_value { get; set; }
        public string Unit {get; set; } = string.Empty;
        public short Quality { get; set; } = 192;
    }
}
