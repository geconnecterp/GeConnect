using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	[Serializable]
	public class JsonDeTRDto
	{
		[JsonProperty(nameof(pi))]
		public List<JsonDeTRDetalleDto> pi { get; set; }

		public JsonDeTRDto()
		{
			pi = [];
		}
	}
}
