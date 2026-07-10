using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class TagSetting
    {
        public int ArrayIndex { get; set; }
        public string Equip_Id { get; set; }
        public string Address { get; set; }
        public string Metric_Name { get; set; }
    }
}
