
namespace gc.infraestructura.Dtos
{
	public class ComisionesDeRepartidoresDetalleDto : Dto
	{
		public string or_compte { get; set; } = string.Empty;
		public string pc_compte { get; set; } = string.Empty;
		public DateTime pc_fecha { get; set; }
		public string rp_id { get; set; } = string.Empty;
		public string rp_nombre { get; set; } = string.Empty;
		public string ve_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public decimal cm_total { get; set; }
		public decimal rp_comi { get; set; }
		public decimal rp_comi_porc { get; set; }
		public decimal rp_comi_base { get; set; }
		public DateTime pc_entrega { get; set; }
	}
}
