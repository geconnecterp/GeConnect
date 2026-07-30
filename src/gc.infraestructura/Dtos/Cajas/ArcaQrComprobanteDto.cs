using System.Text.Json.Serialization;

namespace gc.infraestructura.Dtos.Cajas
{
    public class ArcaQrComprobanteDto
    {
        [JsonPropertyName("ver")]
        public int Ver { get; set; } = 1;

        [JsonPropertyName("fecha")]
        public string Fecha { get; set; } = string.Empty;

        [JsonPropertyName("cuit")]
        public long Cuit { get; set; }

        [JsonPropertyName("ptoVta")]
        public int PtoVta { get; set; }

        [JsonPropertyName("tipoCmp")]
        public int TipoCmp { get; set; }

        [JsonPropertyName("nroCmp")]
        public long NroCmp { get; set; }

        [JsonPropertyName("importe")]
        public decimal Importe { get; set; }

        [JsonPropertyName("moneda")]
        public string Moneda { get; set; } = "PES";

        [JsonPropertyName("ctz")]
        public decimal Ctz { get; set; } = 1m;

        [JsonPropertyName("tipoDocRec")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TipoDocRec { get; set; }

        [JsonPropertyName("nroDocRec")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? NroDocRec { get; set; }

        [JsonPropertyName("tipoCodAut")]
        public string TipoCodAut { get; set; } = "E";

        [JsonPropertyName("codAut")]
        public long CodAut { get; set; }
    }
}