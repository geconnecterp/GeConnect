using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ProductosEnCierreModel
	{
		public GridCoreSmart<ProductoEnCierreDto> GrillaProductos { get; set; }
		public bool MostrarConteoGrupo2 { get; set; } = false;
	}
}
