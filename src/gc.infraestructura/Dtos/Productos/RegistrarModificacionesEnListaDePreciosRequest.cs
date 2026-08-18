using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos
{
	public class RegistrarModificacionesEnListaDePreciosRequest : RequestBase
	{
		public string abm { get; set; }
		public string lpId { get; set; }
		public decimal lpMargen { get; set; }
		public string lpMgnPrincipal { get; set; }
		public decimal lpMgnPrincipalPorc { get; set; }
		public decimal lpPrevisionTot { get; set; }
		public decimal lpPrevisionPin { get; set; }
		public string jsonRubCta { get; set; }
	}
}
