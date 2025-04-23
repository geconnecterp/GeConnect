using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante
{
	public class TabComprobanteModel
	{
		public GridCoreSmart<CompteValorizaListaDto> GrillaValoracion { get; set; }
		public GridCoreSmart<CompteValorizaDtosListaDto> GrillaDescuentosFin { get; set; }
	}
}
