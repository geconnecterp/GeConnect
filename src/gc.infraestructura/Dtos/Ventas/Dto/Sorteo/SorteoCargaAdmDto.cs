
namespace gc.infraestructura.Dtos.Ventas
{
	public class SorteoCargaAdmDto : Dto
	{
		public string so_sorteo { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string so_nro_desde { get; set; } = string.Empty;
		public string so_nro_hasta { get; set; } = string.Empty;
		public int? so_numerador { get; set; }
		public bool incluido { get; set; }
	}
}
