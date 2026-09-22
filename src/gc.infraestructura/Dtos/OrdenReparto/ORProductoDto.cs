namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class ORProductoDto
    {
        public string or_compte { get; set; } = string.Empty;
        public short? item { get; set; }
        public string remplazo { get; set; } = "N";
        public string remplazo_box { get; set; } = string.Empty;
        public string remplazo_p_id { get; set; } = string.Empty;
        public string remplazo_usu_id { get; set; } = string.Empty;
        public decimal colectado_otros { get; set; }
        public decimal colectado_x_box { get; set; }
        public decimal colectado_x_p { get; set; }
        public decimal colectado_remplazo { get; set; }
        public string resultado { get; set; } = "PE";
        public string resultado_msj { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
        public string rubg_id { get; set; } = string.Empty;
        public string rubg_desc { get; set; } = string.Empty;
        public string box_id { get; set; } = string.Empty;
        public string depo_id { get; set; } = string.Empty;
        public string depo_nombre { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;
        public decimal pedido { get; set; }
        public decimal colectado { get; set; }
        public decimal bulto { get; set; }
        public decimal us { get; set; }
        public int unidad_pres { get; set; }
        public string nota { get; set; } = string.Empty;
    }

    public class OrCtlProductoDto
    {
        public string or_compte { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;
        public string p_id_prov { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public char adm_min_excluye { get; set; }
        public char adm_may_excluye { get; set; }
        public int unidad_pres { get; set; }
        public int bultos { get; set; }
        public decimal us { get; set; }
        public decimal cantidad { get; set; }
        public DateTime vto { get; set; }
        public decimal cantidad_total { get; set; }
        public decimal diferencia { get; set; }
    }
}
