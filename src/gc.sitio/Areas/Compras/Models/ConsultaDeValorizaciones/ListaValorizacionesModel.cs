using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaValorizacionesModel
	{
		public GridCoreSmart<ConsultaValorizacionesRegistradasDto> Grilla { get; set; }
	}
}
