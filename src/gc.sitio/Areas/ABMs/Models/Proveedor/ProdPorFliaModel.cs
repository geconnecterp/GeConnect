using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProdPorFliaModel
	{
		public GridCoreSmart<ProductoListaDto> ProductosPorFamilia { get; set; }
	}
}
