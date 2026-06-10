using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Ventas.Models.VentaSorteoConsulta
{
	public class SorteoDatosModel
	{
		public SorteoCargaDatosDto Datos { get; set; }
		public GridCoreSmart<SorteoCargaProdDto> Productos { get; set; }
		public GridCoreSmart<SorteoCargaAdmDto> Sucursales { get; set; }
	}
}
