
namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class ProyEgrDetailDto : Dto
	{
		public DateTime fecha { get; set; }
		public string concepto { get; set; } = string.Empty;
		public decimal importe { get; set; } = 0.00M;
	}
}
