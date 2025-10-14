using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CargarDatosExtractoYSistemaModel
	{
		public string CuentaBanco { get; set; } = string.Empty;
		public decimal Extracto { get; set; } = 0.00M;
		public decimal Sistema { get; set; } = 0.00M;
		public decimal Diferencia { get; set; } = 0.00M;
		public GridCoreSmart<RegistroExtractoDto> GrillaExtracto { get; set; }
		public GridCoreSmart<RegistroSistemaDto> GrillaSistema { get; set; }
	}
}
