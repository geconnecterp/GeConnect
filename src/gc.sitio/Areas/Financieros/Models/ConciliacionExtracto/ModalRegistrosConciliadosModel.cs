using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ModalRegistrosConciliadosModel
	{
		public string RegistroConciliado { get; set; } = string.Empty;
		public int ConciliadoNro { get; set; }
		public GridCoreSmart<RegistroExtractoDto> GrillaExtracto { get; set; }
		public GridCoreSmart<RegistroSistemaDto> GrillaSistema { get; set; }
	}
}
