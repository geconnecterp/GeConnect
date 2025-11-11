using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LiquidacionDeEmpleadoDetalleModel
	{
		public string Leyenda { get; set; } = string.Empty;
		public GridCoreSmart<LiqEmpleadoDetalleParaReporteDto> GrillaLiqDeEmpDetalle { get; set; }
	}
}
