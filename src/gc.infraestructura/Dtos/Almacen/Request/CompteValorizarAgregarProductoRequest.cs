
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizarAgregarProductoRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string rp_compte { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public decimal cantidad { get; set; } = 0.000M;
		public bool incluye_rp { get; set; }
	}
}
