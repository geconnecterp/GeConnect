using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoMovStkServicio : IServicio<TipoMovStkDto>
	{
		List<TipoMovStkDto> ObtenerTiposDeMovimientosDeStock(string token);
	}
}
