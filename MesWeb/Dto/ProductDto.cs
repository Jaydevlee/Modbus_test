namespace MesWeb.Dto
{
    public class ProductDto
    {
        public string ProductId {get; set;} = string.Empty;
        public string Name {get; set;} = string.Empty;
        public string RecipeVersion {get; set;} = string.Empty;
        public bool IsActive {get; set;} = true;
    }
}