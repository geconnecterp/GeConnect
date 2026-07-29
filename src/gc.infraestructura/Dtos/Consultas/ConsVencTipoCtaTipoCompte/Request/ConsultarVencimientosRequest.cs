using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte
{
	public class ConsultarVencimientosRequest : BaseFilters
	{
		public bool fv { get; set; }
		public DateTime fvDesde { get; set; }
		public DateTime fvhasta { get; set; }
		public bool fg { get; set; }
		public DateTime fgDesde { get; set; }
		public DateTime fghasta { get; set; }
		public bool id_ctc { get; set; }
		public List<string> ctc_list { get; set; } = [];
		public bool id_ope { get; set; }
		public List<string> ope_list { get; set; } = [];
		public bool id_tco { get; set; }
		public List<string> tco_list { get; set; } = [];
		public string ctc_list_textos { get; set; } = string.Empty;
		public string ope_list_textos { get; set; } = string.Empty;
		public string tco_list_text { get; set; } = string.Empty;
		public int? Registros { get; set; }
		public int? Pagina { get; set; }
	}
}
