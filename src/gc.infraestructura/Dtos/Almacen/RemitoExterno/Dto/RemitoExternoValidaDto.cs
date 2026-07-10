
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos
{
	public class RemitoExternoValidaDto : Dto, IProductoConUnidad
	{
		public string pre_id { get; set; } = string.Empty;
		public string pre_descripcion { get; set; } = string.Empty;
		public DateTime pre_fecha { get; set; }
		public string pre_nombre { get; set; } = string.Empty;
		public string pre_domicilio { get; set; } = string.Empty;
		public DateTime pre_vigencia_desde { get; set; }
		public DateTime pre_vigencia_hasta { get; set; }
		public string pre_obs_pago { get; set; } = string.Empty;
		public string pre_obs_entrega { get; set; } = string.Empty;
		public char pre_stk_pend { get; set; }
		public char pre_impreso { get; set; }
		public string pree_id { get; set; } = string.Empty;
		public string pree_desc { get; set; } = string.Empty;
		public string pret_id { get; set; } = string.Empty;
		public string pret_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public decimal pre_cantidad { get; set; }
		public decimal pre_cantidad_ent { get; set; }
		public decimal pre_pvta { get; set; }
		public decimal pre_pneto { get; set; }
		public decimal pre_pmargen { get; set; }
		public decimal iva_alicuota { get; set; }
		public int pre_item { get; set; }
		public decimal pre_pcosto { get; set; }
		public decimal a_remitir { get; set; }
		public string box_id { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public decimal bulto { get; set; }
		public decimal us { get; set; }
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}

	public class ProductoRemitoDto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public decimal bulto { get; set; }
		public decimal us { get; set; }
		public decimal cantidad { get; set; }
	}
}
