
namespace gc.infraestructura.Dtos.OrdenDePago.Dtos
{
	public class OPValidacionPrevDto : Dto
	{
		public string msj { get; set; } = string.Empty;
		public string tipo { get; set; } = string.Empty;
		public bool cancelar { get; set; } = false;
	}
}
