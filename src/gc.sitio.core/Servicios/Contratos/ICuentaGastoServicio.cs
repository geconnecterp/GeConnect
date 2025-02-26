using gc.infraestructura.Dtos.CuentaComercial;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ICuentaGastoServicio : IServicio<CuentaGastoDto>
	{
		List<CuentaGastoDto> GetCuentaDirectaParaABM(string ctagId, string token);
	}
}
