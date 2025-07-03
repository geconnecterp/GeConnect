using gc.api.core.Entidades.Tipos;
using gc.infraestructura.Dtos.Tipos;

namespace gc.api.core.Contratos.Servicios.Tipos
{
	public interface ITipoOrdenDePagoServicio : IServicio<TipoOrdenDePago>
	{
		List<TipoOrdenDePagoDto> GetTiposDeOrdenDePago();
	}
}
