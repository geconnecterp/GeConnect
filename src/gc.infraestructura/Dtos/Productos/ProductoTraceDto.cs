namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoTraceDto
    {
        public DateTime fecha { get; set; }
        public string p_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public decimal p_pneto_old { get; set; }
        public decimal p_costo_old { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_costo { get; set; }
        public string p_desc { get; set; } = string.Empty;
    }
}
