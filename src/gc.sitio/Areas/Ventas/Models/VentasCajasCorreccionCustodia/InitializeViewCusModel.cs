using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionCustodia
{
	public class InitializeViewCusModel
	{
		public string Sucursal { get; set; } = string.Empty;
		public GridCoreSmart<VtasPVCtlEntregaDto> GrillaVtasPVCtlEntrega { get; set; }
		public GridCoreSmart<VtasPVCtlEntregaRendDto> GrillaVtasPVCtlEntregaRend { get; set; }
		public string TipoEntrega { get; set; } = string.Empty;
	}
}
