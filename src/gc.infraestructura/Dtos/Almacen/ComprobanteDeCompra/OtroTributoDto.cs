
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class OtroTributoDto : Dto
	{
		public string ins_id { get; set; } = string.Empty; //Valor que se usa para poder identificar un elemento de la colección
		public string imp { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "base")]
		public decimal base_imp { get; set; } = 0.00M; //Al armar el json esta columna se debe llamar 'base'
		public decimal alicuota { get; set; } = 0.00M;
		public decimal importe { get; set; } = 0.00M;
	}
}
