
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ExtractoCrudItemRequest
	{
		public int orden { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public DateTime ext_fecha_movi { get; set; }
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public string ext_concepto { get; set; } = string.Empty;
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
		public string abm { get; set; } = string.Empty;
		public bool insertar { get; set; }
	}
}
