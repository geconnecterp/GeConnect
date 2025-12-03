namespace gc.infraestructura.Dtos.Productos.Precio
{
    public class PrecioListaDetalleDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string pg_id { get; set; } = string.Empty;
        public string pg_desc { get; set; } = string.Empty;
        public string sec_id { get; set; } = string.Empty;
        public string sec_desc { get; set; } = string.Empty;
        public string rubg_id { get; set; } = string.Empty;
        public string rubg_desc { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
        public string iva_situacion { get; set; } = string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_pcosto { get; set; }
        public DateTime p_actu_fecha { get; set; }
        public decimal p_pvta1 { get; set; }
        public string lp_desc1 { get; set; } = string.Empty;
        public decimal p_pvta2 { get; set; }
        public string lp_desc2 { get; set; } = string.Empty;
        public decimal p_pvta3 { get; set; }
        public string lp_desc3 { get; set; } = string.Empty;
        public decimal p_pvta4 { get; set; }
        public string lp_desc4 { get; set; } = string.Empty;
        public string infoImp
        {
            get
            {
                return $"{iva_situacion}-{iva_alicuota}";
            }
        }

    }
}
