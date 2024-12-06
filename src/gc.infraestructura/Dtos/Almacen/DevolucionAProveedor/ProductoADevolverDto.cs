using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	[Serializable]
	[DataContract]
	public class ProductoADevolverDto : Dto
	{
		[DataMember]
		[JsonProperty("p_id")]
		public string p_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("p_desc")]
		public string p_desc { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("cta_id")]
		public string cta_id { get; set; } = string.Empty;
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
		[JsonProperty("dv_compte_revierte")]
		public string dv_compte_revierte { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("depo_id")]
		public string depo_id { get; set; } = string.Empty;
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
