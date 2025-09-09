using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LibroBancoResumenModel
	{
		public GridCoreSmart<LibroBancoResumenDto> GrillaCuentaFin { get; set; }
		public GridCoreSmart<LibroBancoResumenDto> GrillaCuentaBan { get; set; }
	}

}
