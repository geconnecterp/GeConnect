namespace gc.infraestructura.Dtos.Productos.Ofertas
{
    public class ComboProductoDto
    {
        public string cmb_id { get; set; }=string.Empty;
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public decimal p_pcosto { get; set; }
        public int cantidad { get; set; }
        public decimal dto_porc { get; set; }
        public decimal dto_imp { get; set; }
        public string up_id { get; set; } = string.Empty;
        public char activo { get; set; }
    }
}
