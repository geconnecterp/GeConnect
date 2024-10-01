
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAutPIDetalleDto : Dto
	{
		public string pi_compte { get; set; } = string.Empty;
		public string pid_item { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal pid_cantidad { get; set; } = 0;
		public decimal pid_enviado { get; set; } = 0;
        public decimal pid_stk { get; set; } = 0;
    }
}
