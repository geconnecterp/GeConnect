
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class ProductoAAjustarDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public decimal as_stock { get; set; } = 0.000M;
		public decimal as_ajuste { get; set; } = 0.000M;
		public decimal as_resultado { get; set; } = 0.000M;
		public string tipo { get; set; } = string.Empty;
		public string as_nro_revierte { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string ta_id { get; set; } = string.Empty;
		public string nota { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public int bulto { get; set; }
		public decimal us { get; set; } = 0.000M;
		public decimal cantidad { get; set; } = 0.000M;
	}
}
