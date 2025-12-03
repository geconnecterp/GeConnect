using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Mstk.Request
{
	public class ConsultarStockRequest : QueryFilters
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
		public int agrupador { get; set; }
	}
}
