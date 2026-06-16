using System.Security.AccessControl;
using System.Security.Policy;

namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class FactPendienteResponseDto
    {
        public string tco_id { get; set; } = string.Empty;
        public string cm_compte { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string tdoc_id { get; set; } = string.Empty;
        public string cta_documento { get; set; } = string.Empty;
        public string cm_nombre { get; set; } = string.Empty;
        public DateTime cm_fecha { get; set; }
        public decimal cm_total { get; set; }


    }
}
