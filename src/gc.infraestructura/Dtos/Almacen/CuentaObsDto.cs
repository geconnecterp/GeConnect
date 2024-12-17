
namespace gc.infraestructura.Dtos.Almacen
{
	public class CuentaObsDto : Dto
	{
        public string cta_id { get; set; } = string.Empty;
        public string to_id { get; set; } = string.Empty;
		public string to_desc { get; set; } = string.Empty;
		public string to_lista { get; set; } = string.Empty;
		public string cta_obs { get; set; } = string.Empty;
	}
}
