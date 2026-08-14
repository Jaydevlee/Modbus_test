using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class AppConfig
    {
        public DBSettings? DBSettings { get; set; }
        public PlcSettings PlcSettings { get; set; } = new();
        public TagSettings? TagSettings { get; set; }
    }
}
