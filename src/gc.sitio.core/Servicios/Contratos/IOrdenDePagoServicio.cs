using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public  interface IOrdenDePagoServicio : IServicio<OrdenDePagoDto>
	{
		List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id, string token);
		List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId, string token);
	}
}
