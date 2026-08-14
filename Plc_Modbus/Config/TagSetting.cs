using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class TagSetting
    {
        public int ArrayIndex { get; set; }
        public string Equip_Id { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Metric_Name { get; set; } = string.Empty;
        public string DataType { get; set; } = "UInt16";
        public double Scale { get; set; } = 1.0;
        public string Unit { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }
}
