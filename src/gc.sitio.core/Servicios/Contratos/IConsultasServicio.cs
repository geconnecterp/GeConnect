using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IConsultasServicio:IServicio<ConsultasDto>
    {
        Task<(List<ConsCtaCteDto>, MetadataGrid)> ConsultarCuentaCorriente(string ctaId, long fechaD, string userId, int pagina, int registros, string token);
        Task<RespuestaGenerica<ConsVtoDto>> ConsultaVencimientoComprobantesNoImputados(string ctaId, long fechaD, long fechaH, string userId, string token);
        Task<RespuestaGenerica<ConsCompTotDto>> ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId, string token);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctaId"></param>
        /// <param name="mes">el formato de este parametro es aaaamm</param>
        /// <param name="relCuit"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<RespuestaGenerica<ConsCompDetDto>> ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId, string token);
        Task<RespuestaGenerica<ConsOrdPagosDto>> ConsultaOrdenesDePagoProveedor(string ctaId, DateTime fd, DateTime fh, string tipoOP, string userId, string token);
        Task<RespuestaGenerica<ConsOrdPagosDetDto>> ConsultaOrdenesDePagoProveedorDetalle(string cmptId, string token);
        Task<RespuestaGenerica<ConsRecepcionProveedorDto>> ConsultaRecepcionProveedor(string ctaId, DateTime fd, DateTime fh, string admId, string token);
        Task<RespuestaGenerica<ConsRecepcionProveedorDetalleDto>> ConsultaRecepcionProveedorDetalle(string cmptId, string token);
		List<CertRetenGananDto> ConsultaCertRetenGA(string op_compte, string token);
        List<CertRetenGananDto> ConsultaCertRetenGAFromList(string op_compte, string token);
		List<CertRetenIBDto> ConsultaCertRetenIB(string op_compte, string token);
        List<CertRetenIBDto> ConsultaCertRetenIBFromList(string op_compte, string token);
		List<CertRetenIVADto> ConsultaCertRetenIVA(string op_compte, string token);
        List<CertRetenIVADto> ConsultaCertRetenIVAFromList(string op_compte, string token);
		Task<(List<VencimientoListaDto>, MetadataGrid)> ConsultarVencimientos(ConsultarVencimientosRequest filters, string token);
		Task<(List<CertificadoListaDto>, MetadataGrid)> ConsultarCertificados(ConsultarCertificadosRequest filters, string token);
		Task<(List<ProductoStkDto>, MetadataGrid)> ConsultarProductoStk(ConsultarStockRequest filters, string token);
		Task<(List<ProductoStkDto>, MetadataGrid)> ConsultarProductoStkValor(ConsultarStockValorizadoRequest filters, string token);
		Task<(List<ProductoStkCompensadoDto>, MetadataGrid)> ConsultarProductoStkCompensado(ConsultarStockCompensadoRequest filters, string token);
        List<MovimientoListaDto> ConsultaMovimientoLista(BuscarMovDeCuentaDirectaRequest request, string token);
        List<SaldoDetalleDto> BuscarSaldoDetalleCtaDistribuidora(BuscarSaldoDetalleRequest request, string token);
        List<SaldoResumenDto> BuscarSaldoResumenCtaDistribuidora(BuscarSaldoDetalleRequest request, string token);
        List<ComisionesDeVendedoresDetalleDto> BuscarComisionDeVendedorDetalle(ComisionesDeVendedoresRequest request, string token);
        List<ComisionesDeVendedoresResumenDto> BuscarComisionDeVendedorResumen(ComisionesDeVendedoresRequest request, string token);
        List<ComisionesDeRepartidoresDetalleDto> BuscarComisionDeRepartidorDetalle(ComisionesDeRepartidoresRequest request, string token);
        List<ComisionesDeRepartidoresResumenDto> BuscarComisionDeRepartidorResumen(ComisionesDeRepartidoresRequest request, string token);
        List<RepRkgRentabVtasDto> RepRkgRentabVtas(ReporteRankingRentabVtasRequest request, string token);
        List<ReporteEvoVtasPerAnterioresDto> RepEvoVtasPerAnteriores(ReporteEvoVtasPerAnterioresRequest request, string token);
	}
}

