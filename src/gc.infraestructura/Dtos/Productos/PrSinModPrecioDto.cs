namespace gc.infraestructura.Dtos.Productos
{
    public class ProvSinModPrecioDto
    {
        public string cta_id { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public DateTime pg_fecha_cambio_precios { get; set; }
    }
}
