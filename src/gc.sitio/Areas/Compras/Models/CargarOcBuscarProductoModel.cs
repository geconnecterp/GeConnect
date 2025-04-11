using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class CargarOcBuscarProductoModel
	{
		public SelectList ComboSucursales { get; set; }
		public GridCore<ProductoNCPIDto> grillaDatos { get; set; }
	}
}
