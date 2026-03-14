using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class OrdenDeRepartoListaDto : OrdenDeRepartoDto
	{
		public int Total_registros { get; set; } = 0;
		public int Total_paginas { get; set; } = 0;
	}
	public class OrdenDeRepartoDto : Dto
	{
		public string or_compte { get; set; } = string.Empty;
		public string or_obs { get; set; } = string.Empty;
		public DateTime or_fecha { get; set; }
		public DateTime or_fecha_fac { get; set; }
		public char ore_id { get; set; }
		public string ore_desc { get; set; } = string.Empty;
		public string rp_id { get; set; } = string.Empty;
		public string rp_nombre { get; set; } = string.Empty;
	}

	public class PedidoEnOrdenDeRepartoDto : OrdenDeRepartoDto
	{
		public string pc_compte { get; set; } = string.Empty;
		public string pc_obs { get; set; } = string.Empty;
		public DateTime pc_fecha { get; set; }
		public DateTime pc_entrega { get; set; }
		public string pce_id { get; set; } = string.Empty;
		public string pce_desc { get; set; } = string.Empty;
		public string ve_id { get; set; } = string.Empty;
		public string ve_nombre { get; set; } = string.Empty;
		public char pc_cf { get; set; }
		public bool pc_cons_final
		{
			get => char.ToUpper(pc_cf) == 'S';
			set => pc_cf = value ? 'S' : 'N';
		}
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_domicilio { get; set; } = string.Empty;
		public string cta_celu { get; set; } = string.Empty;
		public string cta_email { get; set; } = string.Empty;
		public string cta_te { get; set; } = string.Empty;
		public string zn_id { get; set; } = string.Empty;
		public string zn_desc { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public char? cm_repetido { get; set; }
		public decimal cm_total { get; set; } = 0.00M;
		public decimal ve_comi { get; set; } = 0.00M;
		public decimal ve_comi_base { get; set; } = 0.00M;
		public decimal ve_comi_porc { get; set; } = 0.00M;
		public decimal rp_comi { get; set; } = 0.00M;
		public decimal rp_comi_base { get; set; } = 0.00M;
		public decimal rp_comi_porc { get; set; } = 0.00M;
		public decimal pc_precio_tot { get; set; } = 0.00M;
		public bool mostrar_up { get; set; } = false;
		public bool mostrar_down { get; set; } = false;
	}
}
