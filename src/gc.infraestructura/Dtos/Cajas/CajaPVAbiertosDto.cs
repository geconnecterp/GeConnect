
namespace gc.infraestructura.Dtos.Cajas
{
	public class CajaPVAbiertosDto : Dto
	{
		public string caja_id { get; set; } = string.Empty;
		public string caja_nombre { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string dia_movi { get; set; } = string.Empty;
		public string caja_estado { get; set; } = string.Empty;
		public DateTime caja_apertura { get; set; }
		public DateTime caja_cierre { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
	}
}
