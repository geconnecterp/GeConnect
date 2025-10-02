using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Financieros
{
	public class CrudExtractoBancarioDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public string ext_concepto { get; set; } = string.Empty;
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
		public decimal ext_saldo { get; set; } = 0.00M;
		public string ct_tipo { get; set; } = string.Empty;
		public string ct_modo { get; set; } = string.Empty;
		public string abm { get; set; } = string.Empty;
	}
}
