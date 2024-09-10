using gc.infraestructura.Dtos.Almacen;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	[DataContract]
	[Serializable]
	public class JsonComprobanteDeRPDto
	{
		[DataMember]
		[JsonProperty("tco_id")]
		public string Tco_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("tco_desc")]
		public string Tco_desc { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("cm_compte")]
		public string Cm_compte { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("cm_fecha")]
		public string Cm_fecha { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("cm_importe")]
		public string Cm_importe { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("p_id")]
		public string P_id { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("p_id_prov")]
		public string P_id_prov { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("p_id_barrado")]
		public string P_id_barrado { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("p_desc")]
		public string P_desc { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("bulto_up")]
		public string Bulto_up { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("bulto")]
		public string Bulto { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("uni_suelta")]
		public string Uni_suelta { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("cantidad")]
		public string Cantidad { get; set; } = string.Empty;

		[DataMember]
		[JsonProperty("oc_compte")]
		public string Oc_compte { get; set; } = string.Empty;
		
		[JsonProperty(nameof(Producto))]
		public ProductoBusquedaDto Producto { get; set; }

    }
}
