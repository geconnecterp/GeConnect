using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.OrdenDePago.Dtos
{
	public class RetencionesDesdeObligYCredDto : Dto
	{
		public string imp_id { get; set; } = string.Empty;
		public string imp_desc { get; set; } = string.Empty;
		public string concepto_retencion { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "base")]
		public decimal base_retencion { get; set; } = 0.00M;
		public decimal ali { get; set; } = 0.00M;
		public decimal retencion { get; set; } = 0.00M;
		public string id { get; set; } = string.Empty;
		public string id_desc { get; set; } = string.Empty;
		public decimal base_exenta { get; set; } = 0.00M;
		public decimal nrnp_porc { get; set; } = 0.00M;
		public int resultado { get; set; }
		public string resultado_msj { get; set; } = string.Empty;
	}
}
