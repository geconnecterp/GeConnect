namespace gc.infraestructura.Dtos.Productos.Etiqueta
{
    public class IEDetalleDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_desc { get; set; }=string.Empty;
        public DateTime? p_impreso_fecha { get; set; }
    }
}
