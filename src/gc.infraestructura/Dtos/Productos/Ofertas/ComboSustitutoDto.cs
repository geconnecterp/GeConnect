namespace gc.infraestructura.Dtos.Productos.Ofertas
{
    public class ComboSustitutoDto
    {
        public string cmb_id { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_id_sustituto { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal p_pcosto { get; set; }
        public char activo { get; set; }
    }
}
