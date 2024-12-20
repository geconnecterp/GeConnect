
namespace gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI
{
	public class InfoProductoDto : Dto
	{
        public string p_id { get; set; }=string.Empty;
        public string p_id_barrado_ean { get; set; } = string.Empty;
        public string p_id_barrado_dun { get; set; }= string.Empty;
        public string p_id_prov { get; set; } = string.Empty;
        public int? unidad_presentacion { get; set; }
        public int? unidad_palet { get; set; }
        public char p_con_vto { get; set; }
        public int? p_con_vto_min { get; set; }
        public decimal? p_peso { get; set; } = 0.000M;
        public string up_id { get; set; } = string.Empty;
        public char up_tipo { get; set; }
        public string up_desc { get; set; } = string.Empty;
        public char p_balanza { get; set; }
        public int? p_balanza_dvto { get; set; }
        public string p_balanza_id { get; set; } = string.Empty;
        public char p_activo { get; set; }
        public string p_activo_des { get; set; } = string.Empty;
        public decimal p_plista { get; set; } = 0.000M;
        public decimal p_dto1 { get; set; } = 0.00M;
		public decimal p_dto2 { get; set; } = 0.00M;
		public decimal p_dto3 { get; set; } = 0.00M;
		public decimal p_dto4 { get; set; } = 0.00M;
		public decimal p_dto5 { get; set; } = 0.00M;
		public decimal p_dto6 { get; set; } = 0.00M;
        public decimal p_dto_pa { get; set; } = 0.00M;
        public string p_boni { get; set; } = string.Empty;
        public decimal p_porc_flete { get; set; } = 0.00M;
        public decimal in_alicuota { get; set; } = 0.00M;
        public decimal iva_alicuota { get; set; } = 0.00M;
        public char iva_situacion { get; set; }
        public decimal p_pcosto { get; set; } = 0.000M;
        public decimal p_pcosto_repo { get; set; } = 0.000M;
        public decimal p_pneto_may { get; set; } = 0.000M;
        public decimal p_margen_may { get; set; } = 0.00M;
        public decimal p_pvta_may { get; set; } = 0.00M;
        public decimal p_pneto_min { get; set; } = 0.000M;
        public decimal p_margen_min { get; set; } = 0.00M;
        public decimal p_pvta_min { get; set; } = 0.00M;
        public DateTime? p_actu_fecha { get; set; }
        public string usu_id { get; set; } = string.Empty;
        public string usu_apellidoynombre { get; set; } = string.Empty;
        public string obs_compra { get; set; } = string.Empty;
        public string rp_compte { get; set; } = string.Empty;
        public DateTime? rp_fecha { get; set; }
    }
}
