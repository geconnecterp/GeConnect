using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Remito
{
	public class RemitoDetalle : Dto
	{
		public GridCoreSmart<RemitoVerConteoDto> Productos { get; set; }
		public string rem_compte { get; set; } = string.Empty;
		public string Remito { get; set; } = string.Empty;
		public string QuienEnvia { get; set; } = string.Empty;
		public GridCoreSmart<RTRxULDto> ConteosxUL { get; set; }
		public RemitoDetalle()
		{
			Productos = new GridCoreSmart<RemitoVerConteoDto>();
			ConteosxUL = new GridCoreSmart<RTRxULDto>();
		}
	}
}
