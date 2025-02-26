using gc.api.core.Entidades;
using gc.infraestructura.Dtos.CuentaComercial;

namespace gc.api.core.Contratos.Servicios
{
    public interface ICuentaGastoServicio : IServicio<CuentaGasto>
	{
		List<CuentaGastoDto> GetCuentaGastoParaABM(string ctag_id);
	}
}
