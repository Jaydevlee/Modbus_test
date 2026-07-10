using System;
using System.Collections.Generic;
using System.Text;

namespace Plc_Modbus.Config
{
    public class DBSettings
    {
        public string? Host { get; set; }
        public string? Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Database { get; set; }
    }
}
