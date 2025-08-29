using gc.api.core.Entidades.Tipos;
using gc.infraestructura.Dtos.Tipos;

namespace gc.api.core.Contratos.Servicios.Tipos
{
	public interface ITipoTransferenciaServicio : IServicio<TipoTransferencia>
	{
		List<TipoTransferenciaDto> GetTipoTransferenciaLista();
	}
}
