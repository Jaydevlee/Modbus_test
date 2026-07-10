using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class AppConfig
    {
        public DBSettings? dbSettings { get; set; }
        public TagSetting? tagSetting { get; set; }
    }
}
