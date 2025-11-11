
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class TRValidarTransferenciaRequest : RequestBase
	{
        public string ti { get; set; } = string.Empty;
	}
}
