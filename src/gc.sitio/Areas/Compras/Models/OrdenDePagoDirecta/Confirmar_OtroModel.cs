using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class Confirmar_OtroModel
	{
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string imp { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "base")]
		public decimal base_imp { get; set; } = 0.00M; //Al armar el json esta columna se debe llamar 'base'
		public decimal alicuota { get; set; } = 0.00M;
		public decimal importe { get; set; } = 0.00M;
	}
}
