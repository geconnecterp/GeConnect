
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
    public class ModificarOCRequest : RequestBase
    {
		public string oc_compte { get; set; } = string.Empty;	
		public string adm_id_entrega { get; set; } = string.Empty;
		public AccionesSobreLasOC accion { get; set; }
	}
}
