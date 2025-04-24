using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class SectorABMRubroModel
	{
		public GridCoreSmart<RubroListaABMDto> SectorRubro { get; set; }
        public RubroListaABMDto Rubro { get; set; }
		public SelectList ComboSubSector { get; set; }
		public SectorABMRubroModel()
		{ 
			Rubro = new RubroListaABMDto();
		}
	}
}
