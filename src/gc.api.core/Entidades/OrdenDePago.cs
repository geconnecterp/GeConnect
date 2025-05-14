
namespace gc.api.core.Entidades
{
	public class OrdenDePago : EntidadBase
	{
		public string op_compte { get; set; } = string.Empty;
		public decimal op_importe { get; set; } = 0.00M;
		public DateTime op_fecha { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string opt_id { get; set; } = string.Empty;
		public string? cta_id { get; set; } = string.Empty;
		public string? op_concepto { get; set; } = string.Empty;
		public char? op_impreso { get; set; }
		public char op_anulada { get; set; }
		public DateTime op_anulada_fecha { get; set; }
		public string? op_anulada_usu { get; set; }
	}
}
