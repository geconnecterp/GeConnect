
namespace gc.infraestructura.Dtos.Mstk
{
	public class ProductoStkDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string titulo_repo { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public int p_unidad_pres { get; set; }
		public string p_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; }	= string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public int? p_orden_pg { get; set; }
		public decimal p_pcosto { get; set; } = 0.00M;
		public decimal p_pcosto_repo { get; set; } = 0.00M;
		public string p_activo { get; set; } = string.Empty;
		public string p_activo_des { get; set; } = string.Empty;
		public DateTime? rp_fecha { get; set; }
		public int? rp_dias { get; set; }
		public decimal stk { get; set; } = 0.00M;
		public DateTime? stk_ult_mov { get; set; }
		private bool _p_activo_bool;

		public bool p_activo_bool
		{
			get { return p_activo == "D" ? false : true; }
			set { _p_activo_bool = value; }
		}
	}
}
