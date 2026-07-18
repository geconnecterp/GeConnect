
namespace gc.infraestructura.Dtos.Consultas
{
	public class ReporteRankingRentabVtasRequest
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public List<string>? lSuc { get; set; }
		public List<string>? lProv { get; set; }
		public List<string>? lFam { get; set; }
		public List<string>? lRub { get; set; }

		public int agrupador { get; set; }
	}
}
