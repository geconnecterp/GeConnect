
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class DevolucionRevertidoDto : Dto
	{
        public string dv_compte { get; set; } = string.Empty;
        public DateTime dv_fecha { get; set; }
        public string dv_motivo { get; set; } = string.Empty;
		public string dve_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public decimal dvd_pcosto { get; set; } = 0.000M;
		public decimal dvd_cantidad { get; set; } = 0.000M;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal ps_bulto { get; set; } = 0.000M;
		public string vto { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
        public string depo_nombre { get; set; } = string.Empty;
        public string as_compte_ori { get; set; } = string.Empty;
        public decimal as_stock { get; set; } = 0.000M;
        public decimal as_ajuste { get; set; } = 0.000M;
        public decimal as_resultado { get; set; } = 0.000M;
		public string cta_id { get; set; } = string.Empty;
    }
}
