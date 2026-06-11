
namespace gc.infraestructura.Dtos
{
	public class SorteoComptesDto : Dto
	{
		public string so_sorteo { get; set; } = string.Empty;
		public string so_nro { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string cm_repetido { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public string caja_nro_cierre { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
	}
}
