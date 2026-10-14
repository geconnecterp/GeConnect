
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Mstk
{
	public class PlanillaElabYFraccionDto : Dto, IProductoConUnidad
	{
		public string ep_compte { get; set; } = string.Empty;
		public DateTime ep_fecha { get; set; }
		public string ep_motivo { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string epd_tipo { get; set; } = string.Empty;
		public decimal epd_cantidad { get; set; }
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}
}
