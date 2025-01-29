using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class SectorABMSubSectorModel
	{
		public GridCore<SubSectorDto> SectorSubSector { get; set; }
        public SubSectorModel SubSector { get; set; }
		public SectorABMSubSectorModel()
		{
			SubSector = new SubSectorModel();
		}
	}
}
