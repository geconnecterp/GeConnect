using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IFinancieroServicio : IServicio<FinancieroDto>
	{
		List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id, string token);
		List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id, string token);
		List<FinancieroEstadoDto> GetFinancierosEstados(string token);
		List<PlanContableDto> GetPlanContableCuentaLista(string token);
		List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id, string adm_id, string token);
		List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string token);
		RespuestaGenerica<RespuestaDto> FinancieroConfirmarTransferencia(ConfirmarTransferenciaRequest request, string token);
	}
}
