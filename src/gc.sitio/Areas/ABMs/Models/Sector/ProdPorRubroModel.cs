using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProdPorRubroModel
	{
		public GridCore<ProductoListaDto> ProductosPorRubro { get; set; }
	}
}
