
namespace gc.api.core.Entidades
{
    public class Remito : EntidadBase
    {
        public string rem_compte { get; set; } = string.Empty;
        public string rem_nombre { get; set; } = string.Empty;
        public string rem_domicilio { get; set; } = string.Empty;
        public string tdoc_id { get; set; } = string.Empty;
        public string rem_cuit { get; set; } = string.Empty;
        public DateTime rem_fecha { get; set; }
        public string rem_obs { get; set; } = string.Empty;
        public string rem_retirado_por { get; set; } = string.Empty;
        public char reme_id { get; set; }
        public string adm_id { get; set; } = string.Empty;
        public string depo_id { get; set; } = string.Empty;
        public string pre_id { get; set; } = string.Empty;
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string pv_compte { get; set; } = string.Empty;
    }
}