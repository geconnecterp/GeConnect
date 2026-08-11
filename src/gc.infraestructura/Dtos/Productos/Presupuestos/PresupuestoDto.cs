namespace gc.infraestructura.Dtos.Productos.Presupuestos
{
    public class PresupuestoListDto: PresupuestoDto
    {
        public int Total_registros { get; set; } = 0;
        public int Total_paginas { get; set; } = 0;
    }
    public class PresupuestoDto
    {
        public string pre_id { get; set; } = string.Empty;
        public string pre_descripcion { get; set; } = string.Empty;
        public DateTime pre_fecha { get; set; }
        public string pre_nombre { get; set; } = string.Empty;
        public string pre_domicilio { get; set; } = string.Empty;
        public DateTime pre_vigencia_desde { get; set; }
        public DateTime pre_vigencia_hasta { get; set; }
        public string pre_obs_pago { get; set; } = string.Empty;
        public string pre_obs_entrega { get; set; } = string.Empty;
        //estados
        public char pree_id { get; set; }
        public string pree_desc { get; set; } = string.Empty;
        //tipos
        public char pret_id { get; set; }
        public string pret_desc { get; set; } = string.Empty;

        public string? cta_id { get; set; }
        public string cta_denominacion { get; set; } = string.Empty;
        

        public string usu_id { get; set; } = string.Empty;
        public string usu_apellidoynombre { get; set; } = string.Empty;

        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;

    }

    public class PresupuestoProductoDto:PresupuestoDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_des { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;
        public short pre_item { get; set; }
        public char iva_situacion { get; set; }
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal pre_cantidad { get; set; }
        public decimal pre_pcosto { get; set; }
        public decimal pre_pneto { get; set; }
        public decimal pre_pmargen { get; set; }
        public decimal pre_pvta { get; set; }
        public decimal pre_cantidad_ent { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal p_margen_actual { get; set; }
        public decimal p_pvta_actual { get; set; }
        public decimal p_pcosto_actual { get; set; }
        public decimal pre_total { get; set; }
    }
}
