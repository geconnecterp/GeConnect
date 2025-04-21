
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class ConfirmarComprobanteDeCompraRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public Encabezado? encabezado { get; set; }
		public List<Concepto>? concepto { get; set; }
		public List<Otro>? otros { get; set; }
		public List<Asociacion>? asociaciones { get; set; }
	}

	public class Encabezado
	{
		public string cta_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string ope_iva { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string ctap_id_externo { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public string cm_ctl_fiscal { get; set; } = string.Empty;
		public string cm_cae { get; set; } = string.Empty;
		public DateTime cm_cae_vto { get; set; }
		public string mon_codigo { get; set; } = string.Empty;
		public bool ctag_imputa { get; set; }
		public string ctag_id { get; set; } = string.Empty;
		public DateTime cm_pago { get; set; }
		public int cm_cuota { get; set; }
		public string cm_obs { get; set; } = string.Empty;
		public string cm_libro_iva { get; set; } = string.Empty;
		public string rela_opciones { get; set; } = string.Empty;
		public decimal cm_no_gravado { get; set; }
		public decimal cm_exento { get; set; }
		public decimal cm_gravado { get; set; }
		public decimal cm_iva { get; set; }
		public decimal cm_otros_ng { get; set; }
		public decimal cm_ii { get; set; }
		public decimal cm_percepciones { get; set; }
		public decimal cm_total { get; set; }
	}

	public class Concepto
	{
		public string concepto { get; set; } = string.Empty;
		public int cantidad { get; set; }
		public string iva_situacion { get; set; } = string.Empty;
		public decimal iva_alicuota { get; set; }
		public decimal subtotal { get; set; }
		public decimal iva { get; set; }
		public decimal total { get; set; }
	}

	public class  Otro
	{
		public string imp { get; set; } = string.Empty;
		public string tipo { get; set; } = string.Empty;
		public string ctaf_id { get; set; } = string.Empty;
		[JsonProperty("base")]
		public decimal _base { get; set; }
		public decimal alicuota { get; set; }
		public decimal importe { get; set; }
	}

	public class Asociacion
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte_rp { get; set; } = string.Empty;
	}
}
