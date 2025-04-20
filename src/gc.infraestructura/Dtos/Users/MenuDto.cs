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

    public class MenuPpalDto : MenuItemsDto
    {
        public string mnu_app_action { get; set; } = string.Empty;
        public string mnu_app_controller { get; set; } = string.Empty;
        public string mnu_app_area { get; set; } = string.Empty;
        public string mnu_app_imagen { get; set; } = string.Empty;
        public string mnu_app_central { get; set; } = string.Empty;
    }


    public class MenuJsTree
    {
        public List<MenuRoot> menu { get; set; } = new List<MenuRoot>();
        public string menu_id { get; set; } = string.Empty; 
        public string perfil_id { get; set; } = string.Empty;
    }

    /// <summary>
    /// Estructura Ppal para la generacion de arboles de datos jsTree
    /// </summary>
    public class MenuRoot
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
        public List<MenuRoot> children { get; set; } = [];
        public Estado state { get; set; }= new Estado();
        public MenuRootData data { get; set; } = new MenuRootData();
        public string type { get; set; } = string.Empty;
        public string a_attr { get; set; } = string.Empty;
        

    }

    public class Estado
    {
        public bool opened { get; set; } = true;
        public bool disabled { get; set; } = false;
        public bool selected { get; set; } = false;
    }

    public class MenuRootData
    {
        public bool asignado { get; set; } = false;
        public string item_id { get; set; } = string.Empty;
        public string item_padre { get; set; } = string.Empty;
        public string perfil_default { get; set; } = string.Empty;
        public string archivoB64 { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string ajuste_inflacion { get; set; } = string.Empty;
        public  decimal saldo { get; set; }
        public string cuenta { get; set; } = string.Empty;

    }
}
