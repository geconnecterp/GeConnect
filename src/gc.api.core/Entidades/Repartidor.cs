
namespace gc.api.core.Entidades
{
    public class Repartidor : EntidadBase
    {
        public string Rp_id { get; set; } = string.Empty;
        public string Rp_nombre { get; set; } = string.Empty;
        public decimal Rp_comision { get; set; } = 0.000M;
        public string? Rp_te { get; set; }
        public string? Rp_celu { get; set; }
        public string? Rp_mail { get; set; }
        public string Rp_activo { get; set; } = string.Empty;
        public string Rp_actu { get; set; } = string.Empty;
        public string Rp_lista { get; set; } = string.Empty;
    }
}
