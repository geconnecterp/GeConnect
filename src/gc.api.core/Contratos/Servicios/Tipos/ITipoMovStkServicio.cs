using gc.api.core.Entidades.Tipos;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
	public interface ITipoMovStkServicio: IServicio<TipoMovStk>
	{
		List<TipoMovStkDto> ObtenerTiposDeMovimientosDeStock();
	}
}
