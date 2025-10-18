
namespace gc.infraestructura.Dtos.Financieros
{
	public class GastoProyListaDto : Dto
	{
		public int items { get; set; }
		public DateTime fecha { get; set; }
		public string concepto { get; set; } = string.Empty;
		public decimal importe { get; set; } = 0.00M;
	}
}
