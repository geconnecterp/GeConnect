
namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPROrdenDeCompraDto
	{
        public string oc_compte { get; set; }=string.Empty;
        public string oc_fecha { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string oce_id { get; set; } = string.Empty;
        public string oce_desc { get; set; } = string.Empty;
		public bool seleccionado { get; set; }
    }
}
