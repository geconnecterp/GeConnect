using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IFinancieroServicio : IServicio<FinancieroDto>
	{
		List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id, string token);
		List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id, string token);
		List<FinancieroEstadoDto> GetFinancierosEstados(string token);
		List<PlanContableDto> GetPlanContableCuentaLista(string token);
		List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id, string adm_id, string token);
		List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string token, string cta_id = "%");
		RespuestaGenerica<RespuestaDto> FinancieroConfirmarTransferencia(ConfirmarTransferenciaRequest request, string token);
		List<FinancieroCuentaAlCobroRelaDto> GetCuentaAlCobroRela(string ctaf_id, string token);
		List<FinancieroChequeDepositadoDto> GetFinancieroChequeDepositado(string ctaf_id, DateTime fechaDesde, DateTime fechaHasta, string token);
		List<PerfilUserDto> GetFinancieroTraUsu(DateTime fechaDesde, DateTime fechaHasta, string token);
		Task<(List<MovimientoFinancieroListaDto>, MetadataGrid)> BuscarMovimientoFinanciero(ConsultaMovFinancierosRequest filters, string token);
		RespuestaGenerica<RespuestaDto> MovimientoFinancieroAnular(MovimientoFinancieroAnularRequest request, string token);
		List<FinancieroTraRepoCtagDto> GetFinancieroTraRepoCtag(string tra_compte, string token);
		List<FinancieroTraRepoDDto> GetFinancieroTraRepoDDto(string tra_compte, string token);
		List<MovimientoFinancieroListaDto> BuscarMovimientoFinancieroReporte(ConsultaMovFinancierosRequest filtros, string token);
		List<FinancieroBcoExtractoDto> GetFinancieroBcoExtracto(FinancieroBcoExtractoRequest request, string token);
		List<FinancieroBcoCtaCteDto> GetFinancieroBcoCtaCte(FinancieroBcoCtaCteRequest request, string token);
		List<FinancieroBcoLibroResumenDto> GetFinancieroBcoLibroResumen(FinancieroBcoLibroResumenRequest request, string token);
	}
}
