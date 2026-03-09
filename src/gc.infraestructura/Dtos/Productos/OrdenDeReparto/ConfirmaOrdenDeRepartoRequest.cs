using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class ConfirmaOrdenDeRepartoRequest : RequestBase
	{
		public string abm { get; set; } = string.Empty;
		public string or_compte { get; set; } = string.Empty;
		public string or_obs { get; set; } = string.Empty;
		public string rp_id { get; set; } = string.Empty;
		public string json { get; set; } = string.Empty;
		public List<string> pc { get; set; } = [];
	}
}
