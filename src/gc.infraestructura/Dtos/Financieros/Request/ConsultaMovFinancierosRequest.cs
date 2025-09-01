
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ConsultaMovFinancierosRequest : BaseFilters
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public List<string> ctaf_ori_list { get; set; } = [];
		public bool ctaf_ori { get; set; } = false;
		public List<string> ctaf_des_list { get; set; } = [];
		public bool ctaf_des { get; set; } = false;
		public List<string> tipo_list { get; set; } = [];
		public bool tipo { get; set; } = false;
		public List<string> usu_list { get; set; } = [];
		public bool usu { get; set; } = false;
		public int? Registros { get; set; }
		public int? Pagina { get; set; }
	}
}
