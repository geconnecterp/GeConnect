using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ValoresModel
	{
		public GridCoreSmart<ValoresDesdeObligYCredDto> Grilla { get; set; }
		public bool YaExiste { get; set; } = false;
		public string MensajeExiste { get; set; } = string.Empty;
	}
}
