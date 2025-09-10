
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoVencChequeEmitidoDto : Dto
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public int semana { get; set; }
		public decimal che_emi_nent { get; set; } = 0.00M;
		public decimal che_emi_ent { get; set; } = 0.00M;
		public decimal che_depo { get; set; } = 0.00M;
		public decimal saldo_bco { get; set; } = 0.00M;
	}
}
