using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ReasignacionModel
	{
		public string Pg_Id { get; set; } = string.Empty;
        public SelectList FamiliaProductos { get; set; }
		public GridCoreSmart<InfoProductoFamiliaDto> ProductosPorFamilia { get; set; }
	}
}
