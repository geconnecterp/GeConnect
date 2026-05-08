
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesDetalleAnualDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public int periodo { get; set; }
		public decimal co_facturacion { get; set; }
		public decimal facturacion_per_ant { get; set; }
		public decimal co_costo { get; set; }
		public decimal diferencia { get; set; }
		public decimal co_iva_vtas { get; set; }
		public decimal co_iva_costo { get; set; }
		public decimal co_gastos { get; set; }
	}
}
