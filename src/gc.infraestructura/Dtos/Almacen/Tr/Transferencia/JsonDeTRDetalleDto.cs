using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	[Serializable]
	[DataContract]
	public class JsonDeTRDetalleDto
	{
		[DataMember]
		[JsonProperty("adm_id")]
		public string adm_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("adm_nombre")]
		public string adm_nombre { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("p_id")]
		public string p_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("p_desc")]
		public string p_desc { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("pedido")]
		public decimal pedido { get; set; }
		[DataMember]
		[JsonProperty("stk")]
		public decimal stk { get; set; }
		[DataMember]
		[JsonProperty("stk_adm")]
		public decimal stk_adm { get; set; }
		[DataMember]
		[JsonProperty("box_id")]
		public string box_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("depo_id")]
		public string depo_id { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("depo_nombre")]
		public string depo_nombre { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("a_transferir")]
		public decimal a_transferir { get; set; }
		[DataMember]
		[JsonProperty("a_transferir_box")]
		public decimal a_transferir_box { get; set; } = 0.000m;
		[DataMember]
		[JsonProperty("fv")]
		public string fv { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("pi_compte")]
		public string pi_compte { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("unidad_palet")]
		public int unidad_palet { get; set; }
		[DataMember]
		[JsonProperty("palet")]
		public decimal palet { get; set; }
		[DataMember]
		[JsonProperty("autorizacion")]
		public int autorizacion { get; set; }
		[DataMember]
		[JsonProperty("p_sustituto")]
		public bool p_sustituto { get; set; }
		[DataMember]
		[JsonProperty("p_id_sustituto")]
		public string p_id_sustituto { get; set; } = string.Empty;
		[DataMember]
		[JsonProperty("nota")]
		public string nota { get; set; } = string.Empty;
	}
}
