namespace gc.sitio.Areas.ABMs.Models
{
	public class MPCuentaFinYContableAbmValidationModel
	{
        public string ctaf_id { get; set; }
        public string ctaf_denominacion { get; set; }
        public string ctaf_lista { get; set; }
        public string ctaf_activo { get; set; }
        public string ctaf_estado { get; set; }
        public string ctaf_estado_des { get; set; }
        public decimal ctaf_saldo { get; set; }
        public string adm_id { get; set; }
        public string tcf_id { get; set; }
        public string tcf_desc { get; set; }
        public string ins_id { get; set; }
        public string ins_desc { get; set; }
        public string ccb_id { get; set; }
        public string ccb_id_diferido { get; set; }
        public string ctag_id { get; set; }
        public string mon_codigo { get; set; }
        public string cta_id { get; set; }
    }
}
