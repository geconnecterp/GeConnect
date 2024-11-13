
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class AjusteRevertidoDto : Dto
	{
        public string as_compte { get; set; } = string.Empty;
        public DateTime as_fecha { get; set; }
        public string as_motivo { get; set; } = string.Empty;
		public string at_id { get; set; } = string.Empty;
		public string ae_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
        public string depo_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
        public string box_id { get; set; } = string.Empty;
        public string box_desc { get; set; } = string.Empty;
        public string as_compte_ori { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal as_pcosto { get; set; } = 0.000M;
        public decimal as_stock { get; set; } = 0.000M;
        public decimal as_ajuste { get; set; } = 0.000M;
        public decimal as_resultado { get; set; } = 0.000M;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal ps_bulto { get; set; } = 0.000M;
		public DateTime vto { get; set; }
		public string up_id { get; set; } = string.Empty;
	}
}
