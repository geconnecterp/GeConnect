using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class DetalleDevolucionModel
	{
		public GridCoreSmart<DevolucionRevertidoDto> GrillaDetalle { get; set; }
		public string Leyenda { get; set; }
	}
}
