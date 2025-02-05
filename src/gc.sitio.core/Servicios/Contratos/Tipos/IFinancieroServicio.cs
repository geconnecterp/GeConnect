using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IFinancieroServicio : IServicio<FinancieroDto>
	{
		List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id, string token);
		List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id, string token);
		List<FinancieroEstadoDto> GetFinancierosEstados(string token);
		List<PlanContableDto> GetPlanContableCuentaLista(string token);
	}
}
