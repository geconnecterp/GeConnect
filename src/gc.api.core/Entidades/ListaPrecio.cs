
namespace gc.api.core.Entidades
{
    public class ListaPrecio : EntidadBase
    {
        public string Lp_id { get; set; } = string.Empty;
        public string Lp_desc { get; set; } = string.Empty;
        public decimal Lp_margen { get; set; } = 0.000M;
        public string Lp_mgn_principal { get; set; } = string.Empty;
        public decimal Lp_mgn_principal_porc { get; set; } = 0.000M;
        public int Lp_por_defecto { get; set; }
        public decimal Lp_prevision_tot { get; set; } = 0.000M;
        public decimal Lp_prevision_pin { get; set; } = 0.000M;
        public string Lp_actu { get; set; } = string.Empty;
        public string? Lp_lista { get; set; }
    }
}
