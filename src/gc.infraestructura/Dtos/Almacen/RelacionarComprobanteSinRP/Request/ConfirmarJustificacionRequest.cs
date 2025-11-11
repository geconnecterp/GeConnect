
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP
{
	public class ConfirmarJustificacionRequest : RequestBase
	{
		public string cta_id { get; set; } = string.Empty;
		public string json_comptes { get; set; } = string.Empty;
		public string json_rp { get; set; } = string.Empty;
	}

	public class ConfirmarJustificacionAuxiliarRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public List<ComprobanteParaJustificar>? comprobantes { get; set; }
		public List<RpParaJustificar>? rps { get; set; }
	}

	public class ComprobanteParaJustificar()
	{
		public string cta_id { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
	}

	public class RpParaJustificar()
	{
		public string cta_id { get; set; } = string.Empty;
		public string rp_compte { get; set; } = string.Empty;
	}
}
