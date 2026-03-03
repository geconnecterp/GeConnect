
namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class PedidoListDto : PedidoDto
	{
		public int Total_registros { get; set; } = 0;
		public int Total_paginas { get; set; } = 0;
	}

	public class PedidoDto
	{
		public string pc_compte { get; set; } = string.Empty;
		public string pc_obs { get; set; } = string.Empty;
		public DateTime pc_fecha { get; set; }
		public DateTime? pc_entrega { get; set; }
		public char pce_id { get; set; }
		public string pce_desc { get; set; } = string.Empty;
		public string ve_id { get; set; } = string.Empty;
		public string ve_nombre { get; set; } = string.Empty;
		public string rp_id { get; set; }
		public string rp_nombre { get; set; } = string.Empty;
		public char pc_cf { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public char? cm_repetido { get; set; }
		public decimal cm_total { get; set; } = 0.00M;
		public decimal ve_comi { get; set; } = 0.00M;
		public decimal rp_comi { get; set; } = 0.00M;
	}
}

