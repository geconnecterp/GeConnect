using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Contratos.Servicios
{
	public interface ISectorServicio : IServicio<Sector>
	{
		List<SectorDto> GetSectorParaABM(string sec_id);
		List<SubSectorDto> GetSubSectorParaABM(string sec_id);
		List<SubSectorDto> GetSubSector(string rubg_id);
		List<RubroListaABMDto> GetRubroParaABM(string sec_id);
		List<RubroListaABMDto> GetRubro(string rub_id);
	}
}
