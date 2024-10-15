
namespace gc.api.core.Entidades
{
	public class UsuarioMenu : EntidadBase
	{
		public string usu_id { get; set; } = string.Empty;
        public int mnu_id { get; set; }
        public bool mnu_habilitado { get; set; }
        public DateTime mnu_fecha_alta { get; set; }
        public DateTime? mnu_fecha_mod { get; set; }
        public string? usu_id_modi { get; set; } = string.Empty;
        public string mnu_nombre { get; set; } = string.Empty;
    }
}
