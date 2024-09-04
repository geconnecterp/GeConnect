using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	[Serializable]
	public class JsonComprobanteDeRPDto
	{
		[JsonProperty("tco_id")]
		public string Tco_id { get; set; } = string.Empty;
		[JsonProperty("tco_desc")]
		public string Tco_desc { get; set; } = string.Empty;
		[JsonProperty("cm_compte")]
		public string Cm_compte { get; set; } = string.Empty;
		[JsonProperty("cm_fecha")]
		public string Cm_fecha { get; set; } = string.Empty;
		[JsonProperty("cm_importe")]
		public string Cm_importe { get; set; } = string.Empty;
		[JsonProperty("p_id")]
		public string P_id { get; set; } = string.Empty;
		[JsonProperty("p_id_prov")]
		public string P_id_prov { get; set; } = string.Empty;
		[JsonProperty("p_id_barrado")]
		public string P_id_barrado { get; set; } = string.Empty;
		[JsonProperty("p_desc")]
		public string P_desc { get; set; } = string.Empty;
		[JsonProperty("bulto_up")]
		public string Bulto_up { get; set; } = string.Empty;
		[JsonProperty("bulto")]
		public string Bulto { get; set; } = string.Empty;
		[JsonProperty("uni_suelta")]
		public string Uni_suelta { get; set; } = string.Empty;
		[JsonProperty("cantidad")]
		public string Cantidad { get; set; } = string.Empty;
		[JsonProperty("oc_compte")]
		public string Oc_compte { get; set; } = string.Empty;
	}
}
