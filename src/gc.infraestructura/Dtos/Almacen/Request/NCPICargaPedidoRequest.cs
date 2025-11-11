
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class NCPICargaPedidoRequest : RequestBase
	{
        public string tipo { get; set; } = string.Empty;
		public string pId { get; set; } = string.Empty;
		public string tipoCarga { get; set; } = string.Empty;
		public int bultos { get; set; } = 0;
    }
}
