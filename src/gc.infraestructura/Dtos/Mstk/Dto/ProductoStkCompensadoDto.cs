
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Mstk
{
	public class ProductoStkCompensadoDto : Dto, IProductoConUnidad
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
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string pg_id { get; set; } = string.Empty;
		public string pg_desc { get; set; } = string.Empty;
		public int? p_orden_pg { get; set; }
		public string p_activo { get; set; } = string.Empty;
		public string p_activo_des { get; set; } = string.Empty;
		public decimal stk_positivo { get; set; } = 0.00M;
		public decimal st_negativo { get; set; } = 0.00M;
		
		private bool _p_activo_bool;

		public bool p_activo_bool
		{
			get { return p_activo == "D" ? false : true; }
			set { _p_activo_bool = value; }
		}

		private decimal _stk_diferencia;

		public decimal stk_diferencia
		{
			get { return Math.Abs(stk_positivo - st_negativo); }
			set { _stk_diferencia = value; }
		}
		public bool PermiteDecimales => up_tipo == "P";
	}
}
