namespace gc.infraestructura.Dtos.Productos.Discontinuo
{
    public class DiscontinuoDetalleDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_id_ok { get; set; } = string.Empty;
        public string p_desc{ get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string p_activo { get; set; } = string.Empty;
        public string p_activo_desc { get; set; } = string.Empty;
        public decimal stk { get; set; }
        public bool procesado { get; set; }
        public string procesado_desc { get; set; } = string.Empty;

    }
}
