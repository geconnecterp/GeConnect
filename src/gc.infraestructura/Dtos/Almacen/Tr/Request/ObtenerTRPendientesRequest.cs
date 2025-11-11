
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class ObtenerTRPendientesRequest : RequestBase
	{
        public string titId { get; set; } = string.Empty;
    }
}
