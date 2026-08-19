using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Mstk
{
	public class MovStkProductoDto : Dto, IProductoConUnidad
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string sm_fecha { get; set; } = string.Empty;
		public string sm_concepto { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string sm_tipo { get; set; } = string.Empty;
		public string sm_desc { get; set; } = string.Empty;
		public decimal sm_es { get; set; }
		public decimal sm_es_b { get; set; }
		public decimal sm_stk { get; set; }
		public decimal sm_stk_b { get; set; }
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";

		public string FechaFormateada
		{
			get
			{
				if (string.IsNullOrWhiteSpace(sm_fecha))
					return string.Empty;

				// Tomar solo la parte de la fecha (antes del espacio)
				var fecha = sm_fecha.Split(' ')[0];

				if (fecha.Length < 8)
					return string.Empty;

				string anio = fecha.Substring(0, 4);
				string mes = fecha.Substring(4, 2);
				string dia = fecha.Substring(6, 2);

				return $"{dia}/{mes}/{anio.Substring(2)}"; // dd/MM/yy
			}
		}
	}
}
