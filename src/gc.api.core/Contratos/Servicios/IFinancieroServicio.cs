using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;

namespace gc.api.core.Contratos.Servicios
{
    public interface IFinancieroServicio : IServicio<Financiero>
    {
        List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id);
        List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id);
        List<FinancieroEstadoDto> GetFinancieroEstados();
        List<PlanContableDto> GetPlanContableCuentaLista();
        List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id, string adm_id);
        List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string cta_id = "%");
        List<RespuestaDto> FinancieroConfirmarTransferencia(ConfirmarTransferenciaRequest request);
        List<FinancieroTraRepoCtagDto> GetFinancieroTraRepoCtag(string tra_compte);
        List<FinancieroTraRepoDDto> GetFinancieroTraRepoDDto(string tra_compte);
        List<FinancieroCuentaAlCobroRelaDto> GetCuentaAlCobroRela(string ctaf_id);
		List<FinancieroChequeDepositadoDto> GetFinancieroChequeDepositado(FinancieroChequeDepositadoRequest r);
        List<PerfilUserDto> GetFinancieroTraUsu(FinancieroTraUsuRequest request);
        List<MovimientoFinancieroListaDto> BuscarMovimientoFinanciero(ConsultaMovFinancierosRequest filtros);
        List<RespuestaDto> MovimientoFinancieroAnular(MovimientoFinancieroAnularRequest request);
        List<MovimientoFinancieroListaDto> BuscarMovimientoFinancieroReporte(ConsultaMovFinancierosRequest filtros);
		List<FinancieroBcoExtractoDto> GetFinancieroBcoExtracto(FinancieroBcoExtractoRequest request);
        List<FinancieroBcoCtaCteDto> GetFinancieroBcoCtaCte(FinancieroBcoCtaCteRequest request);
	}
}
