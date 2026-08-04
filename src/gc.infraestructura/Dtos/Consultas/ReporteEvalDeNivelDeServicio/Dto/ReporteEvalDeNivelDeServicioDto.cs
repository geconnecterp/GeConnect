
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos
{
	public class ReporteEvalDeNivelDeServicioDto : Dto, IProductoConUnidad
	{
		public string p_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public char? p_alta_rotacion { get; set; }
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public int? p_orden_pg { get; set; }
		public char p_activo { get; set; }
		public string p_activo_des { get; set; } = string.Empty;
		public string rp_compte { get; set; } = string.Empty;
		public DateTime? rp_fecha { get; set; }
		public string re_compte { get; set; } = string.Empty;
		public DateTime? re_fecha { get; set; }
		public decimal stk { get; set; }
		public decimal vta_u30 { get; set; }
		public bool PermiteDecimales => up_tipo == "P";
		public decimal cantidad { get; set; }
		public decimal cantidad_stk { get; set; }
		public decimal ns { get; set; }
		public decimal cantidad_ar { get; set; }
		public decimal cantidad_ar_stk { get; set; }
		public decimal ns_ar { get; set; }
	}
}
