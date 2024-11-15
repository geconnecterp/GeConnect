
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	[Serializable]
	[DataContract]
	public class ProductoAAjustarDto : Dto
	{
		[DataMember]
		[JsonProperty("p_id")]
		public string p_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("p_desc")]
		public string p_desc { get; set; } = string.Empty;
		[JsonProperty("p_id_prov")]
		//[JsonIgnore]
		public string p_id_prov { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("box_id")]
		public string box_id { get; set; } = string.Empty;
		[JsonProperty("box_desc")]
		//[JsonIgnore]
		public string box_desc { get; set; } = string.Empty;
		[JsonProperty("as_stock")]
		//[JsonIgnore]
		public decimal as_stock { get; set; } = 0.000M;
		[JsonProperty("as_ajuste")]
		//[JsonIgnore]
		public decimal as_ajuste { get; set; } = 0.000M;
		[JsonProperty("as_resultado")]
		//[JsonIgnore]
		public decimal as_resultado { get; set; } = 0.000M;
		[DataMember]
		[JsonProperty("tipo")]
		public string tipo { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("as_nro_revierte")]
		public string as_nro_revierte { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("depo_id")]
		public string depo_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("ta_id")]
		public string ta_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("nota")]
		public string nota { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("up_id")]
		public string up_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("usu_id")]
		public string usu_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("unidad_pres")]
		public int unidad_pres { get; set; }
		[DataMember]
		[JsonProperty("bulto")]
		public int bulto { get; set; }
		[DataMember]
		[JsonProperty("us")]
		public decimal us { get; set; } = 0.000M;
		[DataMember]
		[JsonProperty("cantidad")]
		public decimal cantidad { get; set; } = 0.000M;
		[JsonProperty("as_motivo")]
		//[JsonIgnore]
		public string as_motivo { get; set; } = string.Empty;
	}
}
