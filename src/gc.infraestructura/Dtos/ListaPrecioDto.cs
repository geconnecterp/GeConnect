
namespace gc.infraestructura.Dtos
{
    public class ListaPrecioDto : Dto
    {
        public string lp_id { get; set; } = string.Empty;
        public string lp_desc { get; set; } = string.Empty;
        public decimal lp_margen { get; set; } = 0.000M;
        public string lp_mgn_principal { get; set; } = string.Empty;
        public decimal lp_mgn_principal_porc { get; set; } = 0.000M;
        public int lp_por_defecto { get; set; }
        public decimal lp_prevision_tot { get; set; } = 0.000M;
        public decimal lp_prevision_pin { get; set; } = 0.000M;
        public string lp_actu { get; set; } = string.Empty;
        public string? lp_lista { get; set; }
    }
}
