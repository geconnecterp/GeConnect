using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.ControlYCorreccionDeRP
{
	public class InitDetalleDeRecepcionModel
	{
		public GridCoreSmart<RprCtlItemDetalleDto> GrillaDetalle { get; set; }
		public string Proveedor { get; set; } = string.Empty;
		public string Recepcion { get; set; } = string.Empty;
	}
}
