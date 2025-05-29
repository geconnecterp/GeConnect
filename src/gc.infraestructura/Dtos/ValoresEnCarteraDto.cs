
namespace gc.infraestructura.Dtos
{
	public class ValoresEnCarteraDto : Dto
	{
		public DateTime Fecha { get; set; } = DateTime.Now;
		public string Banco { get; set; } = string.Empty;
		public string NroCheque { get; set; } = string.Empty;
		public decimal Importe { get; set; } = 0.00M;
	}
}
