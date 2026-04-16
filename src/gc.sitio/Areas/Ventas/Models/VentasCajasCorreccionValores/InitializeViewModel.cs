using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	public class InitializeViewModel
	{
		public GridCoreSmart<VtasPVCtlCierresDto> GrillaVtasPVCtlCierres { get; set; }
		public GridCoreSmart<VtasPVCtlRendDto> GrillaVtasPVCtlRend { get; set; }
		public GridCoreSmart<VtasPVCtlRendDetalleDto> GrillaVtasPVCtlRendDetalle { get; set; }
		public string Sucursal { get; set; } = string.Empty;
		public string Fecha { get; set; } = string.Empty;
		public string NroProceso { get; set; } = string.Empty;
	}
}
