
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ConsultaAnticipoFinanEmpRequest : BaseFilters
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public bool cta { get; set; } = false;
		public List<string> cta_list { get; set; } = [];
		public bool tipo { get; set; } = false;
		public List<string> tipo_list { get; set; } = [];
		public bool usu { get; set; } = false;
		public List<string> usu_list { get; set; } = [];
		public int? Registros { get; set; }
		public int? Pagina { get; set; }
	}
}
