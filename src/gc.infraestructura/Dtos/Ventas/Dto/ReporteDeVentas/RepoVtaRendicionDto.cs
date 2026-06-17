
namespace gc.infraestructura.Dtos
{
	public class RepoVtaRendicionDto : Dto
	{
		public int caja_nro_rend { get; set; }
		public string rend_tipo { get; set; } = string.Empty;
		public DateTime? rend_fecha { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public string tcf_desc { get; set; } = string.Empty;
		public decimal rendicion { get; set; }
		public decimal fondo { get; set; }
		public int orden { get; set; }
	}
}
