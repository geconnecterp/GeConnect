using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class AjusteDeStockDetalleModel
	{
		public GridCoreSmart<AjusteRevertidoDto> GrillaAjusteDetalle { get; set; }
		public string Leyenda { get; set; }
	}
}
