using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaComprobantesDeValorizacionModel
	{
		public GridCoreSmart<ComptesDeValorizacionDto> Grilla { get; set; }
	}
}
