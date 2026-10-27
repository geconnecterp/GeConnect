using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaDetalleRprModel
	{
		public GridCoreSmart<CompteValorizaDetalleRprListaDto> Grilla { get; set; }
		public string leyenda { get; set; } = string.Empty;
	}
}
