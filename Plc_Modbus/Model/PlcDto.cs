using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Model
{
    public class PlcDto
    {
        public bool coil_val1 { get; set; }
        public bool coil_val2 { get; set; }
        public ushort hold_val { get; set; }
        public ushort input_val1 { get; set; }
        public ushort input_val2 { get; set; }
    }
}
