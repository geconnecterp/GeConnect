
namespace gc.infraestructura.Dtos.Ventas
{
	public class ConfirmarSorteoDto : Dto
	{
		public string Abm { get; set; } // A: alta, B: baja, M: modificacion
		public SorteoCargaDatosDto Datos { get; set; }
		public List<SorteoCargaProdDto> Productos { get; set; }
		public List<SorteoCargaAdmDto> Sucursales { get; set; }
	}
}
