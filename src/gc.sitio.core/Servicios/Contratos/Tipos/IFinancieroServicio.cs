using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
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
		List<FinancieroBcoLibroDto> GetFinancieroBcoLibro(FinancieroBcoLibroRequest request, string token);
		List<FinancieroBcoVencChequeEmitidoDto> GetFinancieroBcoVencChequeEmitido(FinancieroBcoVencChequeEmitidoRequest request, string token);
		List<FinancieroBcoVencChequeEmitidoListaDto> GetFinancieroBcoVencChequeEmitidoLista(FinancieroBcoVencChequeEmitidoListaRequest request, string token);
		List<ChequeEmitidoEstadoDto> GetChequesEmitidosEstadosLista(string token);
		List<ChequeModificadosListaDto> GetChequeModificadosLista(GetChequeModificadosListaRequest request, string token);
		RespuestaGenerica<RespuestaDto> SetChequeModificar(GetChequeModificarListaRequest request, string token);
		RespuestaGenerica<RespuestaDto> SetFechaDeEntrega(RegistrarFechaDeEntregaRequest request, string token);
		RespuestaGenerica<RespuestaDto> SetRechazoDeCheque(RegistrarRechazoDeChequeRequest request, string token);
		List<ECheqDto> GetECheqLista(PasoPrevioECheqRequest request, string token);
		RespuestaGenerica<RespuestaDto> SetExtractoBancarioConfirmar(SetExtractoBancarioConfirmaRequest request, string token);
		List<CrudExtractoBancarioDto> GetBcoExtractoDesdeFile(ExtractoBcoFileRequest request, string token);
		List<FinancieroConciliaDatosDto> GetFinancieroConciliaDatos(FinancieroConciliaDatosRequest request, string token);
		List<FinancieroConciliaNroDto> GetFinancieroConciliaNro(FinancieroConciliaNrosRequest request, string token);
		RespuestaGenerica<RespuestaDto> FinancieroExtractoDesconcilia(FinancieroExtractoDesconciliaRequest request, string token);
		RespuestaGenerica<RespuestaDto> FinancieroConciliacionExtractoConfirmar(FinancieroConciliacionExtractoConfirmarRequest request, string token);
		List<GastoProyListaDto> GetGastosProyLista(string token);
		List<GastoProyListaDto> GetGastosProyDatos(int items, string token);
		List<ProyFinanDto> GetProyeccionFinanciera(BuscarProyFinanRequest request, string token);
		List<SaldoDeCuentaDto> GetSaldoDeCuentas(BuscarSaldoDeCuentasRequest request, string token);
		List<FlujoDeIngresoDto> GetFlujoDeIngreso(BuscarFlujoDeIngresoRequest request, string token);
		RespuestaGenerica<RespuestaDto> FinancieroAnticipoEmpleadoConfirma(CargaAnticipoEmpleadoRequest request, string token);
		List<FinancieroTopeCtaDto> GetFinancieroTopePorCuenta(string cta_id, string token);
		List<AnticipoDetalleDto> GetAnticipoDetalle(string an_compte, string token);
		List<FinancieroUsuarioDto> GetFinancieroUsuarios(GetFinancieroUsuariosRequest request, string token);
		Task<(List<AnticipoFinanEmpListaDto>, MetadataGrid)> BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest filters, string token);
	}
}
