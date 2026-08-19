using gc.infraestructura.Core.EntidadesComunes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Mstk.Request
{
	public class BuscarMovStockProductosRequest : QueryFilters
	{
		public List<string>? lMovTipo { get; set; }
		public List<string>? lDep { get; set; }
		public List<string>? lBox { get; set; }
		public List<string>? lProv { get; set; }
		public string pId { get; set; } = string.Empty;

		public string lMovTipoTextos { get; set; } = string.Empty;
		public string lDepTextos { get; set; } = string.Empty;
		public string lBoxTextos { get; set; } = string.Empty;
		public string lProvTextos { get; set; } = string.Empty;
		public string pIdTextos { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
