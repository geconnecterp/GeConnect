using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoComprobanteServicio: IServicio<TipoComprobanteDto>
	{
		Task<List<TipoComprobanteDto>> BuscarTiposComptesPorCuenta(string cuenta, string token);
	}
}
