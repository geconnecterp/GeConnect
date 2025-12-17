using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InventarioCargaGrillaModel
	{
		public GridCoreSmart<InventarioListaDto> GrillaInventario { get; set; }
	}
}
