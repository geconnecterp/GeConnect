
namespace gc.infraestructura.Dtos.Almacen
{
    public class ComprobanteDeCompraDto : Dto
    {
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string tdoc_desc { get; set; } = string.Empty;
		public string cta_documento { get; set; } = string.Empty;
		public string cta_domicilio { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string afip_desc { get; set; } = string.Empty;
		public string nj_id { get; set; } = string.Empty;
		public string nj_desc { get; set; } = string.Empty;
		public string cta_ib_nro { get; set; } = string.Empty;
		public string ib_id { get; set; } = string.Empty;
		public string ib_desc { get; set; } = string.Empty;
		public DateTime? cta_cuit_vto { get; set; }
		public char tp_id { get; set; }
		public string ctap_id_externo { get; set; } = string.Empty;
		public char ctap_rgan { get; set; }
		public string rgan_id { get; set; } = string.Empty;
		public char rgan_cert { get; set; }
		public DateTime? rgan_cert_vto { get; set; }
		public decimal rgan_porc { get; set; } = 0.00M;
		public char ctap_rib { get; set; }
		public char rib_id { get; set; }
		public char rib_cert { get; set; }
		public DateTime? rib_cert_vto { get; set; }
		public decimal rib_porc { get; set; } = 0.00M;
		public char ctap_ret_iva { get; set; }
		public decimal? ctap_ret_iva_porc { get; set; } = 0.00M;
		public char ctap_per_iva { get; set; }
		public decimal ctap_per_iva_ali { get; set; } = 0.00M;
		public char ctap_per_ib { get; set; }
		public decimal ctap_per_ib_ali { get; set; } = 0.00M;
		public char? ctap_pago_susp { get; set; }
		public char ctap_devolucion { get; set; }
		public char ctap_devolucion_flete { get; set; }
		public char ctap_acuenta_dev { get; set; }
		public string ope_iva { get; set; } = string.Empty;
		public string ope_iva_descripcion { get; set; } = string.Empty;
		public string? ctag_id { get; set; } = string.Empty;
		public string? ctag_denominacion { get; set; } = string.Empty;
		public char ctap_habilitada { get; set; }
		public string libro_iva { get; set; } = string.Empty;
		public string? cm_cae { get; set; } = string.Empty;
		public DateTime? cm_cae_vto { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		//private string _cuit_parcial;

		//public string cuit_parcial
		//{
		//	get { 
		//		return cta_documento[..8]; 
		//	}
		//	//set { 
		//	//	cuit_parcial = cta_documento[..8]; 
		//	//}
		//}

		private string _cuit_parcial;

		public string cuit_parcial
		{
			get
			{
				return cta_documento[..8];
			}
			set { _cuit_parcial = value; }
		}

		//public ComprobanteDeCompraDto() { }
	}
}
