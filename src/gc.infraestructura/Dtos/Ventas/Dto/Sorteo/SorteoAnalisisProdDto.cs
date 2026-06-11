
namespace gc.infraestructura.Dtos
{
	public class SorteoAnalisisProdDto : Dto
	{
		public string so_sorteo { get; set; } = string.Empty;
		public string so_desc { get; set; } = string.Empty;
		public DateTime so_desde { get; set; }
		public DateTime so_hasta { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public int cant_comptes { get; set; }
		public decimal importe_comptes { get; set; }
		public decimal cant_prod { get; set; }
		public decimal importe_prod { get; set; }
	}
}
