
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
        public int oc_entrega_dias { get; set; }
        public DateTime oc_entrega_fecha { get; set; }
        public string usu_id { get; set; } = string.Empty;
		public string oc_pago_ant { get; set; } = string.Empty;
		public DateTime oc_pago_ant_vto { get; set; }
        public decimal oc_dto { get; set; }
        public decimal oc_dto_porc { get; set; }
        public decimal oc_flete_porc { get; set; }
        public decimal oc_flete_importe { get; set; }
        public string oc_observaciones { get; set; } = string.Empty;
		public decimal oc_gravado { get; set; }
        public decimal oc_no_gravado { get; set; }
        public decimal oc_exento { get; set; }
        public decimal oc_percepciones { get; set; }
        public decimal oc_iva { get; set; }
        public decimal oc_in { get; set; }
        public decimal oc_flete_iva { get; set; }
    }
}
