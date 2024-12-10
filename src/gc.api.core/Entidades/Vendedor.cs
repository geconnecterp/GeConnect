
namespace gc.api.core.Entidades
{
    public class Vendedor : EntidadBase
    {
        public string Ve_id { get; set; } = string.Empty;
        public string Ve_nombre { get; set; } = string.Empty;
        public decimal Ve_comision { get; set; } = 0.000M;
        public string? Ve_te { get; set; }
        public string? Ve_celu { get; set; }
        public string? Ve_mail { get; set; }
        public string Ve_activo { get; set; } = string.Empty;
        public string Ve_actu { get; set; } = string.Empty;
        public string Ve_lista { get; set; } = string.Empty;
    }
}
