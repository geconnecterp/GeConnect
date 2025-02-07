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
        public string perfil_id { get; set; } = string.Empty;
    }

    public class MenuJsTree
    {
        public List<MenuRoot> menu { get; set; }
        public string menu_id { get; set; }
        public string perfil_id { get; set; }
    }

    public class MenuRoot
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public List<MenuRoot> children { get; set; } = [];
        public Estado state { get; set; }= new Estado();
        public bool asignado { get; set; } = false;
        public string padre { get; set; }= string.Empty;
        public string ruta { get; set; } = string.Empty;
    }   

    public class Estado
    {
        public bool opened { get; set; } = true;
        public bool disabled { get; set; } = false;
        public bool selected { get; set; } = false;
    }
}
