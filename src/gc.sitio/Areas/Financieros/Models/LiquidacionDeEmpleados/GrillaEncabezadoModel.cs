using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class GrillaEncabezadoModel
	{
		public GridCoreSmart<LiqEmpleadoEncabezadoDto> GrillaEncabezado { get; set; }
	}
}
