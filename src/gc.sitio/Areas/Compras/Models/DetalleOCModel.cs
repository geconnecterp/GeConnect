using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class DetalleOCModel
	{
        public GridCoreSmart<RPROrdenDeCompraDetalleDto> Detalle { get; set; }
        public string OCCompte { get; set; } = string.Empty;
    }
}
