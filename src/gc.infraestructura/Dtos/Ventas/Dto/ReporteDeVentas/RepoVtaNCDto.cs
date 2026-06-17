
namespace gc.infraestructura.Dtos
{
	public class RepoVtaNCDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public decimal co_nota_credito { get; set; }
		public decimal co_devolucion_dinero { get; set; }
		public decimal co_nc { get; set; }
		public string rb_compte { get; set; } = string.Empty;
		public string rb_compte_cobro { get; set; } = string.Empty;
		public string co_tipo { get; set; } = string.Empty;
		public string tco_id_ori { get; set; } = string.Empty;
		public string cm_compte_ori { get; set; } = string.Empty;
		public string rb_nc { get; set; } = string.Empty;
		public string usu_id_autoriza { get; set; } = string.Empty;
		public string usu_apellidoynombre_autoriza { get; set; } = string.Empty;
	}
}
