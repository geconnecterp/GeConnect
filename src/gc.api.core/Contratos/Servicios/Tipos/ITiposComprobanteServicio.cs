using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
	public interface ITiposComprobanteServicio : IServicio<TipoComprobante>
	{
		List<TipoComprobanteDto> GetTipoComprobanteListaPorCuenta(string cuenta);
		List<TipoComprobanteDto> GetTipoComprobanteListaPorTipoAfip(string afip_id);
		List<TipoComprobanteDto> GetTipoComprobanteListaPorTipoAfip(string afip_id, string opt_id);
	}
}
