using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.CuentaComercial
{
	public class RPROrdenDeCompraDetalleDto : RPROrdenDeCompraDto
	{
		public string ocd_item { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public int ocd_unidad_x_bulto { get; set; }
		public decimal ocd_cantidad { get; set; }
		public decimal ocd_plista { get; set; }
		public decimal ocd_dto1 { get; set; }
		public decimal ocd_dto2 { get; set; }
		public decimal ocd_dto3 { get; set; }
		public decimal ocd_dto4 { get; set; }
		public string ocd_boni { get; set; } = string.Empty;
		public int ocd_bonificacion { get; set; }
		public decimal ocd_pcosto { get; set; }
		public decimal in_alicuota { get; set; }
		public decimal iva_alicuota { get; set; }
		public decimal ocd_iva { get; set; }
		public decimal ocd_in { get; set; }
		public int ocd_unidad_pres { get; set; }
		public int ocd_bultos { get; set; }
		public decimal ocd_dto_pa { get; set; }
		public string p_id_prov { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
	}
}
