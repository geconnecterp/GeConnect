
namespace gc.infraestructura.Dtos.Almacen
{
	public class ProductoNCPIDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string p_m_marca { get; set; } = string.Empty;
		public string p_m_desc { get; set; } = string.Empty;
		public string p_m_capacidad { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_alta_rotacion { get; set; } = string.Empty;
		public string p_con_vto { get; set; } = string.Empty;
		public decimal p_peso { get; set; } = 0.000M;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rub_feteado { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public string p_activo { get; set; } = string.Empty; //Discontinuos?
		public string p_activo_des { get; set; } = string.Empty;
		public string adm_may_excluye { get; set; } = string.Empty;
		public string adm_min_excluye { get; set; } = string.Empty;
		public decimal costo { get; set; } = 0.000M;
		public int p_unidad_pres { get; set; }
		public int p_unidad_palet { get; set; }
		public decimal stk { get; set; } = 0.000M;
		public decimal stk_suc { get; set; } = 0.000M;
		public decimal stk_suc_transito { get; set; } = 0.000M;
		public decimal stk_suc_comprometido { get; set; } = 0.000M;
		public int oc_pendiente { get; set; }
		public int pi_pendiente { get; set; }
		public int pedido { get; set; }
		public string pedido_tipo { get; set; } = string.Empty;
		public int cantidad { get; set; }
		public decimal costo_total { get; set; } = 0.000M;
		public decimal paletizado { get; set; } = 0.000M;
		public string Row_color { get; set; } = "#ffffff";
	}

	public class ProductoNCPISustitutoDto : ProductoNCPIDto
	{
		public int orden { get; set; }
	}
}
