
namespace gc.api.core.Entidades
{
    public class Financiero : EntidadBase
    {
        public string Ctaf_id { get; set; } = string.Empty;
        public string Ctaf_denominacion { get; set; } = string.Empty;
        public string Adm_id { get; set; } = string.Empty;
        public string Tcf_id { get; set; } = string.Empty;
        public string? Ins_id { get; set; }
        public string? Mon_codigo { get; set; }
        public string Ctaf_estado { get; set; } = string.Empty;
        public string Ccb_id { get; set; } = string.Empty;
        public string? Ccb_id_diferido { get; set; }
        public string? Ctag_id { get; set; }
        public string? Cta_id { get; set; }
        public string? Ctaf_obs { get; set; }
        public decimal? Ctaf_saldo { get; set; } = 0.000M;
        public DateTime? Ctaf_actu_fecha { get; set; }
        public char Ctaf_actu { get; set; }
        public char Ctaf_activo { get; set; }
        public string Ctaf_lista { get; set; } = string.Empty;
    }
}
