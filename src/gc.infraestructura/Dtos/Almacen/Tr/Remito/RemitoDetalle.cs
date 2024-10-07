using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Remito
{
	public class RemitoDetalle : Dto
	{
		public GridCore<RemitoVerConteoDto> Productos { get; set; }
		public string rem_compte { get; set; } = string.Empty;
        public string Remito { get; set; } = string.Empty;
		public string QuienEnvia { get; set; } = string.Empty;

		public RemitoDetalle()
		{ 
			Productos= new GridCore<RemitoVerConteoDto>();
		}
	}
}
