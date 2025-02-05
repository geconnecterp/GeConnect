using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IMedioDePagoServicio : IServicio<MedioDePagoDto>
	{
		List<MedioDePagoABMDto> GetMedioDePagoParaABM(string insId, string token);
		List<OpcionCuotaDto> GetOpcionesCuota(string insId, string token);
		List<OpcionCuotaDto> GetOpcionCuota(string insId, int cuota, string token);
		List<FinancieroListaDto> GetCuentaFinYContableLista(string insId, string token);
		List<FinancieroListaDto> GetCuentaFinYContable(string ctafId, string token);
		//TODO: Agregar las de Ctaf
	}
}
