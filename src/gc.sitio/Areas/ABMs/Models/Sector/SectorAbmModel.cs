using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class SectorAbmModel
	{
		public SectorDto Sector { get; set; }
		public SectorAbmModel() 
		{ 
			Sector = new SectorDto();
		}
	}
}
