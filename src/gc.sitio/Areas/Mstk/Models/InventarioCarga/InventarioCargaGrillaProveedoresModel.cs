using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InventarioCargaGrillaProveedoresModel
	{
		public GridCoreSmart<ProveedorEnInventarioDto> GrillaProveedores { get; set; }
	}
}
