using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface IFinancieroServicio : IServicio<Financiero>
    {
        List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id);
        List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id);
        List<FinancieroEstadoDto> GetFinancieroEstados();
        List<PlanContableDto> GetPlanContableCuentaLista();
        List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id);
        List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string cta_id);
	}
}
