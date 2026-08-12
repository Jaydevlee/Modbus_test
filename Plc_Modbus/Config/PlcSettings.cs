namespace Plc_Modbus.Config
{
    public class PlcSettings
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public byte UnitId { get; set; } = 1;
        public ushort StartAddress { get; set; } = 0;
        public ushort RegisterCount { get; set; } = 15;
        public int PollIntervalMs { get; set; } = 500;
        public int MetricIntervalMs { get; set; } = 1000;
        public int ConnectTimeoutMs { get; set; } = 3000;
        public int ReadTimeoutMs { get; set; } = 2000;
    }
}
