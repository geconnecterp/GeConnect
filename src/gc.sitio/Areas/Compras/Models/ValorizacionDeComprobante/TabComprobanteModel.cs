using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante
{
	public class TabComprobanteModel
	{
		public GridCore<CompteValorizaListaDto> GrillaValoracion { get; set; }
		public GridCore<CompteValorizaDtosListaDto> GrillaDescuentosFin { get; set; }
	}
}
