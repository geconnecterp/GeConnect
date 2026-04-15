
namespace gc.infraestructura.Dtos.Ventas
{
	public class VtasPVCtlCierresDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public decimal importe_arq { get; set; } = 0.00M;
		public decimal importe_ok { get; set; } = 0.00M;
		public char pendientes { get; set; }
		public bool pendientes_bool => pendientes == 'S';
	}
}
