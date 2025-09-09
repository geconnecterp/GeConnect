using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LibroBancoResumenModel
	{
		public GridCoreSmart<LibroBancoResumenDto> GrillaCuentaFin { get; set; }
		public GridCoreSmart<LibroBancoResumenDto> GrillaCuentaBan { get; set; }
	}

	public class LibroBancoResumenDto : Dto
	{
		public string descripcion { get; set; } = string.Empty;
		public string saldo { get; set; } = string.Empty;
		public bool es_fuente_negrita { get; set; }
		public string background { get; set; } = string.Empty;
		public bool es_header_1 { get; set; } = false;
		public bool es_header_2 { get; set; } = false;
	}
}
