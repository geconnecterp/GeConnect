using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Cajas
{
	public class FaltanteDeCajaDetalleRequest : RequestBase
	{
		public DateTime hasta { get; set; }
	}
}
