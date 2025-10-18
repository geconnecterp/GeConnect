using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ProyeccionDeGastosModel
	{
		public DateTime Fecha { get; set; }
		public string Concepto { get; set; } = string.Empty;
		public decimal Importe { get; set; } = 0.00M;
		public GridCoreSmart<ProyeccionDeGastoDto> GrillaProyeccion { get; set; }
	}
}
