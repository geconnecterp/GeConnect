namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class OrCtlCargaProductoDto
    {
        public int item { get; set; } = 0;
        public string or_compte { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string p_id_prov { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public int unidad_pres { get; set; }
        public int bulto { get; set; }
        public decimal us { get; set; }
        public string vto { get; set; } = string.Empty;
        public decimal cantidad { get; set; }        
    }
}
