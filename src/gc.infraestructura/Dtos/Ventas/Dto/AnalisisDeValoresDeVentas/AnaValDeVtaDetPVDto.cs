
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaValDeVtaDetPVDto : Dto
	{
		public string adm_list { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public string caja_nro_cierre { get; set; } = string.Empty;
		public DateTime dia { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public decimal co_facturacion { get; set; }
		public decimal co_cobranza { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal co_creditos_gen { get; set; }
		public decimal co_creditos_usados { get; set; }
		public decimal cheques { get; set; }
		public decimal efectivos { get; set; }
		public decimal tarjetas { get; set; }
		public decimal bco_transf { get; set; }
		public decimal mutuales { get; set; }
		public decimal vales { get; set; }
		public decimal otros { get; set; }
		public int semana{ get; set; }
	}
}
