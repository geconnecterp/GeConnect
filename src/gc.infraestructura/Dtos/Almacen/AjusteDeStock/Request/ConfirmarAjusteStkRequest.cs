
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request
{
	public class ConfirmarAjusteStkRequest : RequestBase
	{
		public string json { get; set; } = string.Empty;
		public string compteOri { get; set; } = string.Empty;
	}
}
