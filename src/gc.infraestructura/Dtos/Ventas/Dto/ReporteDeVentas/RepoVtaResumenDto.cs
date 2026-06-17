
namespace gc.infraestructura.Dtos
{
	public class RepoVtaResumenDto : Dto
	{
		public string caja_id { get; set; } = string.Empty;
		public string adm_nombre{ get; set; } = string.Empty;
		public DateTime caja_apertura { get; set; }
		public DateTime caja_cierre { get; set; }
		public int caja_nro_operacion { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public decimal co_facturacion { get; set; }
		public decimal co_facturacion_dif { get; set; }
		public decimal co_ctacte { get; set; }
		public decimal co_cobranza { get; set; }
		public decimal co_cobranza_dif { get; set; }
		public decimal co_nota_credito { get; set; }
		public decimal co_creditos_gen { get; set; }
		public decimal co_creditos_usados { get; set; }
		public decimal co_devolucion_dinero { get; set; }
		public decimal co_nota_debito_prov { get; set; }
		public decimal co_reposicion { get; set; }
		public decimal co_vuelto_up { get; set; }
		public decimal co_ingresos { get; set; }
		public decimal total_caja { get; set; }
		public decimal fondo_inicial { get; set; }
		public decimal fondo_final { get; set; }
		public decimal rendicion { get; set; }
		public decimal diferencia { get; set; }
		public int cant_facturacion { get; set; }
		public int cant_facturacion_dif { get; set; }
		public int cant_cobranza { get; set; }
		public int cant_cobranza_dif { get; set; }
		public int cant_cobranza_anu { get; set; }
		public int cant_nota_credito { get; set; }
		public int cant_nota_debito_prov { get; set; }
		public int cant_devolucion_dinero { get; set; }
		public int cant_cambio_ing { get; set; }
		public int cant_ope { get; set; }
		public decimal efectivos { get; set; }
		public decimal cheques { get; set; }
		public decimal tickets { get; set; }
		public decimal tarjetas { get; set; }
		public decimal mutuales { get; set; }
		public decimal vales { get; set; }
		public decimal otros { get; set; }
		public decimal fa_lib { get; set; }
		public decimal nd_lib { get; set; }
		public decimal nc_lib { get; set; }
	}
}
