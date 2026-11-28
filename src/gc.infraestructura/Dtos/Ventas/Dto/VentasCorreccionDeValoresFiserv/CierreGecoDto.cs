
namespace gc.infraestructura.Dtos.Ventas
{
	public class CierreGecoDto : Dto
	{
		public string nro_proceso { get; set; } = string.Empty;
		public string nro_cierre { get; set; } = string.Empty;
		public string nro_renv { get; set; } = string.Empty;
		public string pv { get; set; } = string.Empty;
		public string cajero { get; set; } = string.Empty;
		public string cupones_ok { get; set; } = string.Empty;
		public string cupon_file { get; set; } = string.Empty;
	}
}
