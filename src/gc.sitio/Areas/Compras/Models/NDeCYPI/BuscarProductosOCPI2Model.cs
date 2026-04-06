using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class BuscarProductosOCPI2Model
	{
		public GridCoreSmart<ProductoNCPIDto> ListaDatosProductos { get; set; }
		public string Tipo { get; set; }
	}
}
