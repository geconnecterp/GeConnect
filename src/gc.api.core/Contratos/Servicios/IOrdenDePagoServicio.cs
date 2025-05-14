using gc.api.core.Entidades;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.api.core.Contratos.Servicios
{
	public interface IOrdenDePagoServicio : IServicio<OrdenDePago>
	{
		List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id);
	}
}
