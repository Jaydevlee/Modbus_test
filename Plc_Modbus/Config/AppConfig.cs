using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class AppConfig
    {
        public DBSettings? DBSettings { get; set; }
        public TagSettings? TagSettings { get; set; }
    }
}
