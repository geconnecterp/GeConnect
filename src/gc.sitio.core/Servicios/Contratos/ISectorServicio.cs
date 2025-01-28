using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ISectorServicio : IServicio<SectorDto>
	{
		List<SectorDto> GetSectorParaABM(string secId, string token);
	}
}
