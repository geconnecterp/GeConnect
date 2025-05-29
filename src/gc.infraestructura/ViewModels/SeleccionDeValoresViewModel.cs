using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.ViewModels
{
	public class SeleccionDeValoresViewModel
	{
		public GridCoreSmart<TipoCuentaFinDto> GrillaTipoCuentaFin { get; set; }
		public GridCoreSmart<FinancieroDesdeSeleccionDeTipoDto> GrillaFin { get; set; }
	}
}
