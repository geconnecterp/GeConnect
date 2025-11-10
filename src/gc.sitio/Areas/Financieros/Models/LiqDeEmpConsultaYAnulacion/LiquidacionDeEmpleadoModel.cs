using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LiquidacionDeEmpleadoModel
	{
		public GridCoreSmart<LiqDeEmpleadoListaDto> GrillaLiqDeEmp { get; set; }
	}
}
