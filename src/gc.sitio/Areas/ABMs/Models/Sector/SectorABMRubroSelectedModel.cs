using gc.infraestructura.Dtos.Almacen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class SectorABMRubroSelectedModel
	{
		public RubroListaABMDto Rubro { get; set; }
		public SelectList ComboSubSector { get; set; }
	}
}
