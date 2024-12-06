
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace gc.sitio.Models.Dto
{
    [Serializable]
    [DataContract]
    public class ProductoAAjustarSerializedDto 
    {
        [DataMember]
        [JsonProperty("p_id")]
        public string p_id { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("p_desc")]
        public string p_desc { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("p_id_barrado")]
        public string p_id_barrado { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("p_id_prov")]
        public string p_id_prov { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("box_id")]
        public string box_id { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("tipo")]
        public string tipo { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("as_compte_revierte")]
        public string as_compte_revierte { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("depo_id")]
        public string depo_id { get; set; } = string.Empty;
        [DataMember]
        [JsonProperty("at_id")]
        public string at_id { get; set; } = string.Empty;
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
        [DataMember]
        [JsonProperty("stk")]
        public decimal stk { get; set; } = 0.000M;
        [DataMember]
        [JsonProperty("stk_ajustado")]
        public decimal stk_ajustado { get; set; } = 0.000M;
    }
}
