using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models.ConsultaDevolucionAProveedores
{
	public class DevolucionesModel
	{
		public GridCoreSmart<DevolucionProveedoresListaDto> GrillaDevoluciones { get; set; }
		public string Leyenda { get; set; }
	}
}
