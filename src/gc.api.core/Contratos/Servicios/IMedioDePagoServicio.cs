using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios
{
	public interface IMedioDePagoServicio : IServicio<Instrumento>
	{
		List<MedioDePagoABMDto> GetMedioDePagoParaABM(string ins_id);
		List<OpcionCuotaDto> GetOpcionesDeCuotaParaABM(string ins_id);
		List<OpcionCuotaDto> GetOpcionDeCuotaParaABM(string ins_id, int cuota);
		List<FinancieroListaDto> GetCuentaFinYContableListaParaABM(string ins_id);
		List<FinancieroListaDto> GetCuentaFinYContableParaABM(string ctaf_id);
	}
}
