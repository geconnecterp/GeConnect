using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Mstk.Request
{
	public class ConsultarStockCompensadoRequest : QueryFilters
	{
		public List<string>? lProv { get; set; }
		public List<string>? lRub { get; set; }
		public bool chkEstAct { get; set; }
		public bool chkEstDisc { get; set; }

		public int diferencia { get; set; }
	}
}
