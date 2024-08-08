namespace gc.infraestructura.Dtos.Productos
{

    public class InfoProdBase : Dto
    {
        public string P_id { get; set; } = string.Empty;
        public decimal Ps_stk { get; set; }
        public decimal Ps_bulto { get; set; }
    }
    public class InfoProdExt : InfoProdBase
    {
        public string Depo_id { get; set; } = string.Empty;
        public string Depo_nombre { get; set; } = string.Empty;

        public DateTime? Vto { get; set; }
    }

    public class InfoProdStkD : InfoProdExt
    {
        public string Adm_id { get; set; } = string.Empty;
    }

    public class InfoProdStkBox : InfoProdExt
    {
        public string Box_id { get; set; } = string.Empty;
    }

    public class InfoProdStkA:InfoProdBase
    {
       
        public string Adm_id { get; set; } = string.Empty;
        public string Adm_nombre { get; set; } = string.Empty;
      
    }

    public class InfoProdMovStk
    {
        public string P_id { get; set; }= string.Empty;
        public string Sm_fecha_ind { get; set; } = string.Empty;
        public string sm_concepto { get; set; } = string.Empty;
        public string sm_tipo { get; set; } = string.Empty;
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;
        public string Depo_id { get; set; } = string.Empty;
        public string Depo_nombre { get; set; } = string.Empty;
        public decimal Sm_cantidad { get; set; }
        public decimal Sm_cantidad_b { get; set; }
        public decimal sm_stk { get; set; }
        public decimal sm_stk_b { get; set; }

    }

    public class InfoProdLP
    {
        public string P_id { get; set; } = string.Empty;
        public string Lp_id { get; set; } = string.Empty;
        public string Lp_desc { get; set; } = string.Empty;
        public string Iva_situacion { get; set; } = string.Empty;
        public decimal Iva_alicuota { get; set; }
        public decimal p_iva { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_in { get; set; }
        public decimal p_pcosto { get; set; }
        public decimal p_pcosto_repo { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_pvta { get; set; }
        public int p_unidad_vta { get; set; }
        public DateTime p_actu_fecha { get; set; }
        public string usu_id { get; set; }= string.Empty;
    }
}
