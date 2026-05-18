
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaValDeVtaDetDiarioDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public DateTime dia { get; set; }
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
		public int cheques_cant { get; set; }
		public decimal cheques_cant_porc { get; set; }
		public int efectivos_cant { get; set; }
		public decimal efectivos_cant_porc { get; set; }
		public int tarjetas_cant { get; set; }
		public decimal tarjetas_cant_porc { get; set; }
		public int bco_transf_cant { get; set; }
		public decimal bco_transf_cant_porc { get; set; }
		public int mutuales_cant { get; set; }
		public decimal mutuales_cant_porc { get; set; }
		public int vales_cant { get; set; }
		public decimal vales_cant_porc { get; set; }
		public int otros_cant { get; set; }
		public decimal otros_cant_porc { get; set; }
		public int semana { get; set; }
	}
}
