using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Productos.Ofertas
{
    public class OfertaDto
    {
        public string p_id { get; set; } = string.Empty;//+
        public string p_desc { get; set; } = string.Empty;//+
        public string adm_id { get; set; } = string.Empty;//+
        public string adm_nombre { get; set; } = string.Empty;//+
        public string lp_id { get; set; } = string.Empty;//+
        public string lp_desc { get; set; } = string.Empty;//+
        public string oft_id { get; set; } = string.Empty;
        public string oft_desc { get; set; } = string.Empty;

        public decimal p_pcosto { get; set; }
        public decimal in_alicuota { get; set; }
        public char iva_situacion { get; set; }
        public decimal iva_alicuota { get; set; }
        public decimal p_margen { get; set; }
        public decimal p_pvta { get; set; }
        public decimal p_pneto_vta { get; set; }
        public decimal p_iva { get; set; }
        public decimal p_in { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_margen_oferta { get; set; }
        public decimal p_pvta_oferta { get; set; }
        public DateTime po_fecha_desde { get; set; }
        public DateTime po_fecha_hasta { get; set; }
        public int po_limite { get; set; }
        public int dias { get; set; }
        public DateTime hoy { get; set; }
        public decimal ps_stk { get; set; }        

    }
}
