using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProdPorRubroModel
	{
		public GridCoreSmart<ProductoListaDto> ProductosPorRubro { get; set; }
	}
}
