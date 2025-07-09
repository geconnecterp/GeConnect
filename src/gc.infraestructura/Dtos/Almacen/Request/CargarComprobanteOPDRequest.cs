
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CargarComprobanteOPDRequest
	{
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
	}
}
