using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class EdicionTipoValoresDeTercerosEnCarteraModel : EdicionTipoModel
	{
		public GridCoreSmart<FinancieroCarteraDto> GrillaValoresEnCartera { get; set; }
	}
}
