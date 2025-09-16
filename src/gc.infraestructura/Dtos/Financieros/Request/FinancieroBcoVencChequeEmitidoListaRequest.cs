
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroBcoVencChequeEmitidoListaRequest
	{
		public bool id_f { get; set; } = true;
		public string ctaf_id { get; set; } = string.Empty;
		public bool id_c { get; set; } = false;
		public string cta_id { get; set; } = "%";
		public bool id_u { get; set; } = false;
		public string usu_id { get; set; } = "%";
		public char tipo_fecha { get; set; } = 'E';
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public string estado { get; set; } = "%";
		public string impreso { get; set; } = "%";
	}
}
