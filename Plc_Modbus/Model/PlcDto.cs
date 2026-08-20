namespace Plc_Modbus.Model
{
    public class PlcDto
    {
        public bool IsConnected { get; set; }
        public ushort ProductionCount { get; set; }
        public ushort EquipmentStatus { get; set; }
        public string EquipmentStatusText => EquipmentStatus switch
        {
            1 => "RUN",
            2 => "ERROR",
            _ => "STOP"
        };
        public double CycleTimeSeconds { get; set; }
        public ushort AlarmCode { get; set; }
        public ushort VisionResult { get; set; }
        public string VisionResultText => VisionResult switch
        {
            1 => "OK",
            2 => "NG",
            _ => "미검사"
        };
        public ushort VisionEventSequence { get; set; }
        public int TargetQuantity {get; set;}
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Current { get; set; }
        public double InstantPower { get; set; }
        public double AccumulatedEnergy { get; set; }
        public DateTimeOffset CollectedAt { get; set; }
    }
}
