using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class PresDeValEnCartera_Paso1Model
	{
		public string tpo_medio { get; set; } = string.Empty;
		public SelectList ListaTipoMedioDePago { get; set; }
	}
}
