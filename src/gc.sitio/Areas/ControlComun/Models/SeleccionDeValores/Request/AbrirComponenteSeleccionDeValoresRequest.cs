using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Request
{
	public class AbrirComponenteSeleccionDeValoresRequest
	{
		public string app { get; set; } = string.Empty;
		public decimal importe { get; set; } = 0.00M;
		public string valor_a_nombre_de { get; set; } = string.Empty;
		public List<ValoresDesdeObligYCredDto> valores { get; set; } = [];
	}
}
