using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class PasarAPIModel
	{
		public SelectList ListaSucursales { get; set; }
		public GridCoreSmart<PedidoInternoPendienteDetalleDto> ListaProductos { get; set; }
	}
}
