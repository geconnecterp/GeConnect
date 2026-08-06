using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Productos.Models.ListaDePreciosGestionar
{
	public class ListaDePreciosGestionarModel
	{
		public GridCoreSmart<ListaPrecioDto> GrillaListaDePrecios { get; set; }
		public ListaPrecioDto LPSelected { get; set; } = new ListaPrecioDto();
	}
}
