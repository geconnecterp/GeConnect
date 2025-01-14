
namespace gc.sitio.Areas.ABMs.Models
{
	public class ProveedorAbmValidationModel
	{
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string tdoc_desc { get; set; } = string.Empty;
		public string cta_documento { get; set; } = string.Empty;
		public string cta_domicilio { get; set; } = string.Empty;
		public string cta_localidad { get; set; } = string.Empty;
		public string cta_cpostal { get; set; } = string.Empty;
		public string prov_id { get; set; } = string.Empty;
		public string prov_nombre { get; set; } = string.Empty;
		public string dep_id { get; set; } = string.Empty;
		public string dep_nombre { get; set; } = string.Empty;
		public string cta_www { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string afip_desc { get; set; } = string.Empty;
		public string nj_id { get; set; } = string.Empty;
		public string nj_desc { get; set; } = string.Empty;
		public string cta_ib_nro { get; set; } = string.Empty;
        public string ib_id { get; set; } = string.Empty;
		public string ib_desc { get; set; } = string.Empty;
		public DateTime? cta_alta { get; set; }
        public DateTime? cta_cuit_vto { get; set; }
        public string cta_emp { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public DateTime? cta_emp_ctaf { get; set; }
        public DateTime? cta_actu_fecha { get; set; }
        public string cta_actu { get; set; } = string.Empty;
		public string tp_id { get; set; } = string.Empty;
		public string ctap_ean { get; set; } = string.Empty;
		public string ctap_id_externo { get; set; } = string.Empty;
		public string ctap_rgan { get; set; } = string.Empty;
		public string rgan_id { get; set; } = string.Empty;
		public string rgan_cert { get; set; } = string.Empty;
		public DateTime? rgan_cert_vto { get; set; }
        public decimal? rgan_porc { get; set; }
        public string ctap_rib { get; set; } = string.Empty;
		public string rib_id { get; set; } = string.Empty;
		public string rib_cert { get; set; } = string.Empty;
		public DateTime? rib_cert_vto { get; set; }
        public decimal? rib_porc { get; set; }
        public string ctap_ret_iva { get; set; } = string.Empty;
		public decimal? ctap_ret_iva_porc { get; set; }
        public string ctap_per_iva { get; set; } = string.Empty;
		public decimal? ctap_per_iva_ali { get; set; }
        public string ctap_per_ib { get; set; } = string.Empty;
		public decimal? ctap_per_ib_ali { get; set; }
        public string ctap_pago_susp { get; set; } = string.Empty;
		public string ctap_devolucion { get; set; } = string.Empty;
		public string ctap_devolucion_flete { get; set; } = string.Empty;
		public string ctap_acuenta_dev { get; set; } = string.Empty;
		public decimal? ctap_d1 { get; set; }
		public decimal? ctap_d2 { get; set; }
		public decimal? ctap_d3 { get; set; }
		public decimal? ctap_d4 { get; set; }
		public decimal? ctap_d5 { get; set; }
		public decimal? ctap_d6 { get; set; }
        public string ope_iva { get; set; } = string.Empty;
		public string ope_iva_descripcion { get; set; } = string.Empty;
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_denominacion { get; set; } = string.Empty;
		public string ctap_habilitada { get; set; } = string.Empty;
	}
}
