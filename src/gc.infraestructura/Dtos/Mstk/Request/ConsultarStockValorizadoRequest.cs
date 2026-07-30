using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Mstk.Request
{
	public class ConsultarStockValorizadoRequest : QueryFilters
	{
		public List<string>? lSuc { get; set; }
		public List<string>? lDep { get; set; }
		public List<string>? lProv { get; set; }
		public List<string>? lFam { get; set; }
		public List<string>? lRub { get; set; }

		public bool chkStkPos { get; set; }
		public bool chkStkCero { get; set; }
		public bool chkStkNeg { get; set; }
		public bool chkEstAct { get; set; }
		public bool chkEstDisc { get; set; }
		public bool chkCostoRepo { get; set; }

		public string lSucTextos { get; set; } = string.Empty;
		public string lDepTextos { get; set; } = string.Empty;
		public string lProvTextos { get; set; } = string.Empty;
		public string lRubTextos { get; set; } = string.Empty;
		public string lFamTextos { get; set; } = string.Empty;
		public string chkStockTextos { get; set; } = string.Empty;
		public string chkEstadoTextos { get; set; } = string.Empty;
		public string chkCostoRepoTextos { get; set; } = string.Empty;
		public int agrupador { get; set; }
	}
}
