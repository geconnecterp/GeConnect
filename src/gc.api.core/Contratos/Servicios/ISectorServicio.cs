using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Contratos.Servicios
{
	public interface ISectorServicio : IServicio<Sector>
	{
		List<SectorDto> GetSectorParaABM(string sec_id);
	}
}
