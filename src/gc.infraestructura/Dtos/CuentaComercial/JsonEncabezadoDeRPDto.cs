using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	[Serializable]
	public class JsonEncabezadoDeRPDto
	{
		[JsonProperty("ope")]
		public string Ope { get; set; }=string.Empty;
		[JsonProperty("rp")]
		public string Rp { get; set; } = string.Empty;
		[JsonProperty("cta_id")]
		public string Cta_id { get; set; } = string.Empty;
		[JsonProperty("usu_id")]
		public string Usu_id { get; set; } = string.Empty;
		[JsonProperty("adm_id")]
		public string Adm_id { get; set; } = string.Empty;
		[JsonProperty("depo_id")]
		public string Depo_id { get; set; } = string.Empty;
		[JsonProperty("rpe_id")]
		public string Rpe_id { get; set; } = string.Empty;
		[JsonProperty("rpe_desc")]
		public string Rpe_desc { get; set; } = string.Empty;
		[JsonProperty("nota")]
		public string Nota { get; set; } = string.Empty;
		[JsonProperty("turno")]
		public string Turno { get; set; } = string.Empty;
        [JsonProperty("comprobantes")]
        public List<JsonComprobanteDeRPDto> Comprobantes { get; set; }
    }
}
