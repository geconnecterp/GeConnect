using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ReasignacionRubroModel
	{
		public string Rub_Id { get; set; } = string.Empty;
		public SelectList RubroProductos { get; set; }
		public SelectList RubroProductosAReasignar { get; set; }
		public GridCoreSmart<InfoProductoRubroDto> ProductosPorRubro { get; set; }
	}
}
