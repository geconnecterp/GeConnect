
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroTopeCtaDto : Dto
	{
		public decimal? saldo { get; set; }
	}

	public class FinancieroTopeCtaListaDto : Dto
	{
		public string cta_id { get; set; }
		public decimal? saldo { get; set; }
	}
}
