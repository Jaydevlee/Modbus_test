using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class TagSettings
    {
        public List<TagSetting>? CoilTag { get; set; }
        public List<TagSetting>? HoldingTag { get; set; }
    }
}
