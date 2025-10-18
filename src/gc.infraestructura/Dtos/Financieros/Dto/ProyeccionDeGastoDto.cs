
namespace gc.infraestructura.Dtos.Financieros
{
	public class ProyeccionDeGastoDto : Dto
	{
		public int orden { get; set; }
		public DateTime fecha { get; set; }
		public string concepto { get; set; } = string.Empty;
		public decimal importe { get; set; } = 0.00M;
		public decimal acumulado { get; set; } = 0.00M;
	}
}
