
namespace gc.infraestructura.Dtos.Consultas
{
	public class ReporteVarVtasYCompUltDoceMRequest
	{
		public List<string>? lSuc { get; set; }
		public List<string>? lProv { get; set; }
		public List<string>? lFam { get; set; }
		public List<string>? lRub { get; set; }
		public string lSucTextos { get; set; } = string.Empty;
		public string lProvTextos { get; set; } = string.Empty;
		public string lRubTextos { get; set; } = string.Empty;
		public string lFamTextos { get; set; } = string.Empty;
		public int agrupador { get; set; }
	}
}
