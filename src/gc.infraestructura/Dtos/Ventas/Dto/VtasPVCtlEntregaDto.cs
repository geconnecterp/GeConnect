
namespace gc.infraestructura.Dtos.Ventas
{
	public class VtasPVCtlEntregaDto : Dto
	{
		public string ent_compte { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public DateTime ent_fecha { get; set; }
		public string ent_resp_entrega { get; set; } = string.Empty;
		public string ent_resp_recibe { get; set; } = string.Empty;
		public string ent_compte_retiro { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public decimal ent_importe { get; set; }
		public decimal ent_importe_ctaf { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public char ent_estado { get; set; }
		public string estado_desc { get; set; } = string.Empty;
		public char ent_actu { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public bool ent_actu_bool => ent_actu == 'S';
	}
}
