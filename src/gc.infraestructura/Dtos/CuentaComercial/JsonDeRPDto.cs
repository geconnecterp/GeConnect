using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	[Serializable]
	public class JsonDeRPDto
	{
		[JsonProperty(nameof(encabezado))]
		public List<JsonEncabezadoDeRPDto> encabezado { get; set; }

		public JsonDeRPDto() 
		{
			encabezado = [];
		}
    }


	[Serializable]
	public class JsonDeRPDto2
	{
		[JsonProperty(nameof(encabezado))]
		public List<JsonEncabezadoDeRPDto> encabezado { get; set; }
		[JsonProperty(nameof(comprobantes))]
		public List<JsonComprobanteDeRPDto> comprobantes { get; set; }

		public JsonDeRPDto2()
		{
			encabezado = [];
			comprobantes = [];
		}
	}
}
