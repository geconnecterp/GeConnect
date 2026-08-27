namespace gc.infraestructura.Dtos
{
	public class DetalleDeComprobanteRequest : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
	}
}
