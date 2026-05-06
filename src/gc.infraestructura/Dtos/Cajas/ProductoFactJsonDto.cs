namespace gc.infraestructura.Dtos.Cajas
{
    public class ProductoFactJsonDto
    {
        public string p_id { get; set; }= string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;  
        public decimal p_pcosto { get; set; }
        public decimal p_pcosto_repo { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_in { get; set; }
        public string iva_situacion { get; set; } = string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal p_iva { get; set; }
        public bool po { get; set; }
        public int po_limite { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_margen_imp { get; set; }
        public decimal p_margen_vig { get; set; }
        public decimal p_pvta { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal cantidad_tot { get; set; }
        public decimal p_pvta_tot { get; set; }
        public int bultos { get; set; }
        public decimal cm_gravado { get; set; }
        public decimal cm_no_gravado { get; set; }
        public decimal cm_exento { get; set; }
        public decimal cm_iva { get; set; }
        public decimal cm_ii { get; set; }
        public decimal cm_dto { get; set; }
        public decimal cm_dto_porc { get; set; }
        public string cta_id { get; set; } = string.Empty;
        public string pre_id { get; set; } = string.Empty;
        public string cpf_nro { get; set; } = string.Empty;
        public string cmb_p_id { get; set; } = string.Empty;
        public string cmd_cmb { get; set; } = string.Empty;
        public string cmd_cmb_id { get; set; } = string.Empty;
        public decimal cmd_cmb_dto { get; set; }
        public decimal cmd_cmb_cant { get; set; }
        public string cmd_cmb_desc { get; set; } = string.Empty;
        public string barre { get; set; } = string.Empty;
        public int item { get; set; }
    }
}
