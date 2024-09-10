using gc.infraestructura.Dtos.Almacen;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	[Serializable]
	[DataContract]
	public class JsonEncabezadoDeRPDto
	{
		[DataMember]
		[JsonProperty("ope")]
		public string Ope { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("rp")]
		public string Rp { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("cta_id")]
		public string Cta_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("usu_id")]
		public string Usu_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("adm_id")]
		public string Adm_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("depo_id")]
		public string Depo_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("rpe_id")]
		public string Rpe_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("rpe_desc")]
		public string Rpe_desc { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("nota")]
		public string Nota { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("turno")]
		public string Turno { get; set; } = string.Empty;

		[JsonProperty("tco_id")]
		public string Tco_id { get; set; } = string.Empty;
		[JsonProperty("cm_compte")]
		public string Cm_compte { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("comprobantes")]
		public List<JsonComprobanteDeRPDto> Comprobantes { get; set; }

		public JsonEncabezadoDeRPDto()
		{
			Comprobantes = [];
		}
	}
}
