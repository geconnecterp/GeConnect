
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoLibroResumenDto : Dto
	{
		public decimal saldo_sis { get; set; } = 0.00M;
		public decimal cheques_sis { get; set; } = 0.00M;
		public decimal transferencias_h_sis { get; set; } = 0.00M;
		public decimal creditos_ext { get; set; } = 0.00M;
		public decimal depositos_sis { get; set; } = 0.00M;
		public decimal transferencias_d_sis { get; set; } = 0.00M;
		public decimal debitos_ext { get; set; } = 0.00M;
		public decimal saldo_ext { get; set; } = 0.00M;
	}
}
