using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class EdicionTipoValoresDeTercerosEnCarteraModel : EdicionTipoModel
	{
		public GridCoreSmart<ValoresEnCarteraDto> GrillaValoresEnCartera { get; set; }
	}
}
