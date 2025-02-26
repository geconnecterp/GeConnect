using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoCuentaGastoServicio : IServicio<TipoCuentaGastoDto>
	{
		List<TipoCuentaGastoDto> ObtenerTipoCuentaGasto(string token);
	}
}
