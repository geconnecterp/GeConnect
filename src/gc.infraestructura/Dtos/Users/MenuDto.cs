namespace gc.infraestructura.Dtos.Users
{
    public class MenuDto
    {
        public string mnu_id { get; set; } = string.Empty;
        public string mnu_descripcion { get; set; } = string.Empty;
    }

    public class MenuItemsDto
    {
        public string mnu_id { get; set; } = string.Empty;
        public string mnu_item_id { get; set; } = string.Empty;
        public string mnu_item_padre { get; set; } = string.Empty;
        public string mnu_item { get; set; } = string.Empty;
        public string mnu_item_name { get; set; } = string.Empty; 
        public bool asignado { get; set; }
    }

    public class MenuRoot
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;

        public List<MenuRoot> children { get; set; } = [];
        public bool asignado { get; set; } = false;
        public string ruta { get; set; } = string.Empty;
    }   
}
