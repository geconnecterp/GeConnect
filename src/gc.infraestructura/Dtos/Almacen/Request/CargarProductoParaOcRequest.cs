
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
    public class CargarProductoParaOcRequest : RequestBase
	{
		public string Cta_Id { get; set; } = string.Empty;
		public bool Nueva { get; set; }
		public string Oc_Compte { get; set; } = string.Empty;
	}
}
