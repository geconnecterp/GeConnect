using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ConsultaOCModel
	{
		public GridCore<OrdenDeCompraConsultaDto> GrillaOC { get; set; }
		public SelectList ListaAdministraciones { get; set; }
		public decimal Importe { get; set; }
	}
}
