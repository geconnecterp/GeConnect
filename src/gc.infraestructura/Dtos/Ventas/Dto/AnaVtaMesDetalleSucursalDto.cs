
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesDetalleSucursalDto : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public decimal co_facturacion { get; set; }
		public decimal co_facturacion_porc { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal co_cobranza { get; set; }
		public decimal rentabilidad { get; set; }
		public decimal co_costo { get; set; }
		public decimal diferencia { get; set; }
		public decimal co_iva_vtas { get; set; }
		public decimal co_iva_costo { get; set; }
		public decimal co_gastos { get; set; }
	}
}
