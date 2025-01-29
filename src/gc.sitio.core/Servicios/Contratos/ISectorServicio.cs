using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ISectorServicio : IServicio<SectorDto>
	{
		List<SectorDto> GetSectorParaABM(string secId, string token);
		List<SubSectorDto> GetSubSectorParaABM(string secId, string token);
		List<SubSectorDto> GetSubSector(string rubgId, string token);
		List<RubroListaABMDto> GetRubroParaABM(string secId, string token);
		List<RubroListaABMDto> GetRubro(string rubId, string token);
	}
}
