using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class TabComprobanteModel
	{
		public ComprobanteDeCompraDto Comprobante { get; set; }
		public GridCoreSmart<CompteValorizaListaDto> GrillaValoracion { get; set; }
		public GridCoreSmart<CompteValorizaDtosListaDto> GrillaDescuentosFin { get; set; }
		public SelectList ConceptoDtoFinanc { get; set; }
		public CompteValorizaDtosListaDto DescFinanc { get; set; }
	}
}
