namespace MesWeb.Dto
{
    public class EquipDto
    {
        public string EquipId {get; set;} = string.Empty;
        public string Name {get; set;} = string.Empty;
        public string Location {get; set;} = string.Empty;
        public string Status {get; set;} = string.Empty;
        public bool IsActive {get; set;}
    }
}