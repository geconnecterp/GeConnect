
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public int mes { get; set; }
		public int periodo { get; set; }
		public decimal co_facturacion { get; set; }
		public decimal co_facturacion_acu { get; set; }
		public decimal facturacion_mes_ant { get; set; }
		public decimal mes_ant_dif { get; set; }
		public decimal mes_ant_dif_porc { get; set; }
		public decimal facturacion_per_ant { get; set; }
		public decimal per_ant_dif { get; set; }
		public decimal per_ant_dif_porc { get; set; }
		public decimal co_costo { get; set; }
		public decimal rentabilidad { get; set; }
		public decimal rentabilidad_acu { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal ctacte_dif { get; set; }
		public decimal ctacte_dif_porc { get; set; }
		public decimal diferencia { get; set; }
		public decimal co_iva_vtas { get; set; }
		public decimal co_iva_costo { get; set; }
		public decimal co_gastos { get; set; }
		public decimal co_reposicion { get; set; }

	}
}
