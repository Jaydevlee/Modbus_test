namespace MesWeb.Dto{
    public class PlcDto
    {
        public DateTimeOffset Time {get; set;}
        public string EquipId {get; set;} = string.Empty;
        public string Address {get; set;} = string.Empty;
        public string MetricName {get; set;} = string.Empty;
        public double MetricValue {get; set;}
        public string? Unit {get; set;}
        public short Quality {get; set;}
        public DateTimeOffset CollectedAt {get; set;}
    }
}