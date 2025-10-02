using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CrudExtractoBancarioModel
	{
		public GridCoreSmart<CrudExtractoBancarioDto> GrillaExtracto { get; set; }
		public string CuentaBanco { get; set; } = string.Empty;
	}
}
