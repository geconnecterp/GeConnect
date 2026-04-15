
namespace gc.infraestructura.Dtos.Ventas
{
	public class VtasPVCtlRendDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_rend { get; set; }
		public char rend_tipo { get; set; }
		public string tcf_id { get; set; } = string.Empty;
		public string tcf_desc { get; set; } = string.Empty;
		public decimal rend_importe_arq { get; set; } = 0.00M;
		public decimal rend_importe_ok { get; set; } = 0.00M;
		public char pendientes { get; set; }
		public bool pendientes_bool => pendientes == 'S';
	}
}
