
namespace gc.infraestructura.Dtos.Almacen
{
	public class PedidoInternoPendienteDetalleDto : Dto, IProductoConUnidad
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public string p_orden_pg { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public int p_unidad_pres { get; set; }
		public int p_unidad_palet { get; set; }
		public decimal stk { get; set; }
		public decimal stk_suc { get; set; }
		public int bultos { get; set; }
		public decimal cantidad { get; set; }
		public int pi_pendiente { get; set; }
		public string re_compte { get; set; } = string.Empty;
		public DateTime re_fecha { get; set; }
		public int re_dias { get; set; }
		public bool PermiteDecimales => up_tipo == "P";
		public bool selected { get; set; } = true;
	}
}
