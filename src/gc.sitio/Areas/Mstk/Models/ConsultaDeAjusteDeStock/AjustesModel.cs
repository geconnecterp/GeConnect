using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models.ConsultaDeAjusteDeStock
{
	public class AjustesModel
	{
		public GridCoreSmart<AjusteDeStockListaDto> GrillaAjustes { get; set; }
		public string Leyenda { get; set; }
	}
}
