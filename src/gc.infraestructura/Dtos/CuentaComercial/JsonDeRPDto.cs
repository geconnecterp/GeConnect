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
}
