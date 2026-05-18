
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaValDeVtaMesDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public int mes { get; set; }
		public int periodo { get; set; }
		public decimal co_facturacion { get; set; }
		public decimal co_cobranza { get; set; }
		public decimal co_creditos_gen { get; set; }
		public decimal co_creditos_usados { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal co_ctacte_porc { get; set; }
		public decimal co_ctacte_dist { get; set; }
		public decimal co_ctacte_dist_porc { get; set; }
		public decimal cheques { get; set; }
		public decimal cheques_porc { get; set; }
		public decimal efectivos { get; set; }
		public decimal efectivos_porc { get; set; }
		public decimal tarjetas { get; set; }
		public decimal tarjetas_porc { get; set; }
		public decimal bco_transf { get; set; }
		public decimal bco_transf_porc { get; set; }
		public decimal mutuales { get; set; }
		public decimal mutuales_porc { get; set; }
		public decimal vales { get; set; }
		public decimal vales_porc { get; set; }
		public decimal otros { get; set; }
		public decimal otros_porc { get; set; }
	}
}
