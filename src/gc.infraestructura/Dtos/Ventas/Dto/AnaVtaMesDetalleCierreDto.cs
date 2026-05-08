
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesDetalleCierreDto : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public DateTime caja_apertura { get; set; }
		public decimal co_facturacion { get; set; }
		public decimal co_facturacion_porc { get; set; }
		public decimal co_facturacion_dif { get; set; }
		public decimal co_nota_credito { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal co_cobranza { get; set; }
		public decimal co_cobranza_dif { get; set; }
		public decimal co_creditos_usados { get; set; }
		public decimal rentabilidad { get; set; }
		public decimal co_costo { get; set; }
		public decimal diferencia { get; set; }
		public decimal co_iva_vtas { get; set; }
		public decimal co_iva_costo { get; set; }
		public decimal co_gastos { get; set; }
		public decimal a_rendir { get; set; }
	}
}
