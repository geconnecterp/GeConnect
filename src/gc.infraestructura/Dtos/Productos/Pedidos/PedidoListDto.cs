
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
		public decimal ve_comi_base { get; set; } = 0.00M;
		public decimal ve_comi_por { get; set; } = 0.00M;
		public decimal rp_comi { get; set; } = 0.00M;
		public decimal rp_comi_base { get; set; } = 0.00M;
		public decimal rp_comi_por { get; set; } = 0.00M;
		public bool pc_cons_final
		{
			get => char.ToUpper(pc_cf) == 'S';
			set => pc_cf = value ? 'S' : 'N';
		}
		public string facturado => $"{tco_desc} {cm_compte}";
	}

	public class PedidoProductoDto : PedidoDto
	{
		public int pcd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string lp_id { get; set; } = string.Empty;
		public decimal pcd_pedida { get; set; }
		public decimal pcd_pvta { get; set; }
		public decimal pcd_enviada { get; set; }
		public char pcd_oferta { get; set; }
		public char pcd_origen { get; set; }
		public string p_id_remplazo { get; set; } = string.Empty;
		public decimal ve_comi_base { get; set; }
		public decimal ve_comi_porc { get; set; }
		public decimal rp_comi_base { get; set; }
		public decimal rp_comi_porc { get; set; }
		public bool pcd_origen_bool => pcd_origen == 'P';
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
	}

	public class PedidoElementoDto : Dto 
	{
		public int pcd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string lp_id { get; set; } = string.Empty;
		public decimal pcd_pedida { get; set; }
		public decimal pcd_pvta { get; set; }
		public decimal pcd_enviada { get; set; }
		public char pcd_oferta { get; set; }
		public char pcd_origen { get; set; }
		public string p_id_remplazo { get; set; } = string.Empty;
		public decimal ve_comi_base { get; set; }
		public decimal ve_comi_porc { get; set; }
		public decimal rp_comi_base { get; set; }
		public decimal rp_comi_porc { get; set; }
	}
}

