using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
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
        List<FinancieroBcoLibroResumenDto> GetFinancieroBcoLibroResumen(FinancieroBcoLibroResumenRequest request);
        List<FinancieroBcoLibroDto> GetFinancieroBcoLibro(FinancieroBcoLibroRequest request);
        List<FinancieroBcoVencChequeEmitidoDto> GetFinancieroBcoVencChequeEmitido(FinancieroBcoVencChequeEmitidoRequest request);
        List<FinancieroBcoVencChequeEmitidoListaDto> GetFinancieroBcoVencChequeEmitidoLista(FinancieroBcoVencChequeEmitidoListaRequest request);
        List<ChequeEmitidoEstadoDto> GetChequeEmitidoEstadoLista();
        List<ChequeModificadosListaDto> GetChequeModificadosLista(GetChequeModificadosListaRequest request);
        List<RespuestaDto> SetChequeModificar(GetChequeModificarListaRequest request);
        List<RespuestaDto> SetFechaDeEntrega(RegistrarFechaDeEntregaRequest request);
        List<RespuestaDto> SetRechazoDeCheque(RegistrarRechazoDeChequeRequest request);
        List<ECheqDto> GetECheqLista(PasoPrevioECheqRequest request);
        List<RespuestaDto> SetExtractoBancarioConfirmar(SetExtractoBancarioConfirmaRequest request);
        List<CrudExtractoBancarioDto> GetBcoExtractoDesdeFile(ExtractoBcoFileRequest request);
        List<FinancieroConciliaDatosDto> GetFinancieroConciliaDatos(FinancieroConciliaDatosRequest request);
        List<FinancieroConciliaNroDto> GetFinancieroConciliaNro(FinancieroConciliaNrosRequest request);
		List<RespuestaDto> FinancieroExtractoDesconcilia(FinancieroExtractoDesconciliaRequest request);
        List<RespuestaDto> FinancieroConciliacionExtractoConfirmar(FinancieroConciliacionExtractoConfirmarRequest request);
        List<GastoProyListaDto> GetGastosProyLista();
		List<GastoProyListaDto> GetGastosProyDatos(int items);
        List<ProyFinanDto> GetProyeccionFinanciera(BuscarProyFinanRequest request);
		List<SaldoDeCuentaDto> GetSaldoDeCuentas(BuscarSaldoDeCuentasRequest request);
        List<FlujoDeIngresoDto> GetFlujoDeIngreso(BuscarFlujoDeIngresoRequest request);
        List<RespuestaDto> FinancieroAnticipoEmpleadoConfirma(CargaAnticipoEmpleadoRequest request);
	}
}
