
namespace gc.api.core.Entidades.Tipos
{
	public class TipoImpuesto : EntidadBase
	{
		public string imp_id { get; set; } = string.Empty;
		public string imp_descripcion { get; set; } = string.Empty;
		public char cont { get; set; }
		public char cont_sufre_ret { get; set; }
		public char cont_sufre_ret_bco { get; set; }
		public char cont_sufre_per { get; set; }
		public char cont_sufre_reta { get; set; }
		public char cont_pago_ant { get; set; }
		public char agente_ret { get; set; }
		public char agente_ret_pag_ant { get; set; }
		public char agente_per { get; set; }
		public char agente_per_pag_ant { get; set; }
		public string agente_ret_ccb { get; set; } = string.Empty;
		public string agente_per_ccb { get; set; } = string.Empty;
	}
}
