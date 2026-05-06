
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesDetalleDiarioDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public int semana { get; set; }
		public DateTime dia { get; set; }
		public decimal co_facturacion { get; set; }
		public decimal co_facturacion_acu { get; set; }
		public decimal fac_may_acu_porc { get; set; }
		public decimal co_costo { get; set; }
		public decimal rentabilidad { get; set; }
		public decimal rentabilidad_acu { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal ctacte_dif { get; set; }
		public decimal ctacte_dif_porc { get; set; }
		public decimal co_fac_may { get; set; }
		public decimal co_fac_may_acu { get; set; }
		public decimal co_fac_min { get; set; }
		public decimal co_fac_min_acu { get; set; }
		public decimal co_fac_dis { get; set; }
		public decimal co_fac_dis_acu { get; set; }
		public decimal diferencia { get; set; }
		public decimal co_iva_vtas { get; set; }
		public decimal co_iva_costo { get; set; }
		public decimal co_gastos { get; set; }
		public decimal co_reposicion { get; set; }
		public decimal acumulado_ma { get; set; }
	}
}
