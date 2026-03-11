
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class AnalizarAutOrdenDeRepartoDto : Dto, IProductoConUnidad
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal pedido { get; set; }
		public decimal stk { get; set; }
		public decimal stk_adm { get; set; }
		public string box_id { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public decimal a_enviar { get; set; }
		public decimal a_enviar_box { get; set; }
		public string? fv { get; set; }
		public string pc_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public int unidad_palet { get; set; }
		public decimal palet { get; set; }
		public string or_compte { get; set; } = string.Empty;
		public bool p_sustituto { get; set; }
		public string? p_id_sustituto { get; set; }
		public string? nota { get; set; }
		public string p_id_prov { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}
}
