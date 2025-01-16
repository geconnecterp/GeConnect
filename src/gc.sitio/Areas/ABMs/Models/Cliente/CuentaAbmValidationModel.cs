using gc.infraestructura.Dtos;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaAbmValidationModel : Dto
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
		public string cta_ib_nro { get; set; } = string.Empty;
		public string? ib_id { get; set; } = string.Empty;
		public string ib_desc { get; set; } = string.Empty;
		public DateTime? cta_alta { get; set; }
		public DateTime? cta_cuit_vto { get; set; }
		public string cta_emp { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public string? cta_emp_ctaf { get; set; } = string.Empty;
		public DateTime? cta_actu_fecha { get; set; }
		public string cta_actu { get; set; } = string.Empty;
		public decimal ctac_tope_credito { get; set; } = 0.000M;
		public decimal ctac_tope_credito_dia { get; set; } = 0.000M;
		public decimal ctac_dto_operacion { get; set; } = 0.000M;
		public decimal ctac_dto_operacion_dia { get; set; } = 0.000M;
		public string piva_cert { get; set; } = string.Empty;
		public DateTime? piva_cert_vto { get; set; }
		public string pib_cert { get; set; } = string.Empty;
		public DateTime? pib_cert_vto { get; set; }
		public string ctn_id { get; set; } = string.Empty;
		public string ctn_desc { get; set; } = string.Empty;
		public string ctc_id { get; set; } = string.Empty;
		public decimal? ctac_tope_credito_dia_ult { get; set; }
		public decimal? ctac_dto_operacion_dia_ult { get; set; }
		public string ctc_desc { get; set; } = string.Empty;
		public string? ve_id { get; set; } = string.Empty;
		public string ve_nombre { get; set; } = string.Empty;
		public string? ve_visita { get; set; } = string.Empty;
		public string zn_id { get; set; } = string.Empty;
		public string zn_desc { get; set; } = string.Empty;
		public string? rp_id { get; set; } = string.Empty;
		public string rp_nombre { get; set; } = string.Empty;
		public string ctac_habilitada { get; set; } = string.Empty;
		public string nj_id { get; set; } = string.Empty;
		public string nj_desc { get; set; } = string.Empty;
        public int ctac_ptos_vtas { get; set; }
        public string? ctac_negocio_inicio { get; set; }
        public string lp_id { get; set; } = string.Empty;
		public string lp_desc { get; set; } = string.Empty;
	}
}
