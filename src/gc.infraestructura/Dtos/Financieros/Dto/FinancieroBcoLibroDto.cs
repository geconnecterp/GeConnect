
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoLibroDto : Dto
	{
		public char tipo { get; set; }
		public string ct_tipo { get; set; } = string.Empty;
		public DateTime? fecha { get; set; }
		public DateTime? fecha_vto { get; set; }
		public string concepto { get; set; } = string.Empty;
		public string che_nro { get; set; } = string.Empty;
		public char che_estado { get; set; }
		public int? che_emision { get; set; }
		public decimal importe { get; set; } = 0.00M;
		public decimal saldo_bco { get; set; } = 0.00M;
		public decimal saldo_bco_che { get; set; } = 0.00M;
		public decimal saldo_pendiente { get; set; } = 0.00M;
		public string anombre { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public decimal conciliado_m_ant { get; set; } = 0.00M;
		public decimal conciliado_m_sig { get; set; } = 0.00M;
		public decimal conciliado_m_pos { get; set; } = 0.00M;
	}
}
