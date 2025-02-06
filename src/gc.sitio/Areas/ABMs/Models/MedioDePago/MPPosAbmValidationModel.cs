namespace gc.sitio.Areas.ABMs.Models
{
	public class MPPosAbmValidationModel
	{
		public string ins_id { get; set; }
		public string ins_desc { get; set; }
		public string ins_lista { get; set; }
		public string mon_codigo { get; set; }
		public string ins_dato1_desc { get; set; }
		public string ins_dato2_desc { get; set; }
		public string ins_dato3_desc { get; set; }
		public string ins_detalle { get; set; }
		public decimal ins_comision { get; set; }
		public decimal ins_comision_fija { get; set; }
		public string ins_razon_social { get; set; }
		public string ins_cuit { get; set; }
		public decimal ins_ret_gan { get; set; }
		public decimal ins_ret_ib { get; set; }
		public decimal ins_ret_iva { get; set; }
		public string ins_arqueo { get; set; }
		public string ins_tiene_vto { get; set; }
		public string ins_vigente { get; set; }
		public bool ctaf_id_link_check { get; set; }
		public string ctaf_id_link { get; set; }
		public string tcf_id { get; set; }
		public string tcf_desc { get; set; }
		public string ins_id_pos { get; set; }
		public string ins_id_pos_ctls { get; set; }
	}
}
