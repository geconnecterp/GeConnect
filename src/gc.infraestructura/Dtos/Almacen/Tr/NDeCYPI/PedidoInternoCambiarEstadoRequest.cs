using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class PedidoInternoCambiarEstadoRequest : RequestBase
	{
		public string PiCompte { get; set; } = string.Empty;
		public bool Anula { get; set; }
		public bool Cierra { get; set; }
	}
}
