namespace gc.infraestructura.Dtos.Productos.Etiqueta
{
    public class EtiquetaDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_unidad_pres { get; set; } = string.Empty;
        public char es_oferta { get; set; }
        public char es_feteado { get; set; }
        public string lp_id { get; set; } = string.Empty;
        public string p_pvta_leyenda { get; set; } = string.Empty;
        public decimal p_pvta { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_pvta_real { get; set; }
        public string p_pvta_leyenda2 { get; set; } = string.Empty;
        public decimal p_pvta2 { get; set; }
        public decimal p_pneto2 { get; set; }
        public decimal p_pvta_real2 { get; set; }
        public DateTime hoy { get; set; }
    }
}
