namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class ORProductoDto
    {
        public string ti { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
        public string rubg_id { get; set; } = string.Empty;
        public string rubg_desc { get; set; } = string.Empty;
        public string box_id { get; set; } = string.Empty;
        public string depo_id { get; set; } = string.Empty;
        public string depo_nombre { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal pedido { get; set; }
        public decimal colectado { get; set; }
        public  int bulto { get; set; }
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
