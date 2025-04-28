using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface ICuentaServicio : IServicio<Cuenta>
    {
        //List<ProveedorListaDto> GetProveedorLista();
        List<ProveedorLista> GetProveedorLista(string ope_iva);
        List<CuentaDto> GetCuentaComercialLista(string texto, char tipo);
        List<RPROrdenDeCompraDto> GetOCporCuenta(string cta_id);
        List<RPROrdenDeCompraDetalleDto> GetDetalleDeOC(string oc_compte);
        List<ProveedorFamiliaListaDto> GetProveedorFamiliaLista(string ctaId);
        List<CuentaABMDto> GetCuentaParaABM(string cta_id);
        List<CuentaFPDto> GetCuentaFormaDePago(string cta_id);
        List<CuentaContactoDto> GetCuentContactos(string cta_id);
        List<CuentaObsDto> GetCuentaObs(string cta_id);
        List<CuentaObsDto> GetCuentaObsDatos(string cta_id, string to_id);
		List<CuentaNotaDto> GetCuentaNota(string cta_id);
        List<CuentaFPDto> GetFormaDePagoPorCuentaYFP(string cta_id, string fp_id);
        List<CuentaContactoDto> GetCuentContactosporCuentaYTC(string cta_id, string tc_id);
        List<CuentaNotaDto> GetCuentaNotaDatos(string cta_id, string usu_id);
        List<ProveedorGrupoDto> GetABMProveedorFamiliaLista(string ctaId);
        List<ProveedorGrupoDto> GetABMProveedorFamiliaDatos(string ctaId, string pgId);
        List<ClienteListaDto> GetClienteLista(string search);
        List<ComprobanteDeCompraDto> GetCompteDatosProv(string ctaId);
        List<RprAsociadosDto> GetCompteCargaRprAsoc(string ctaId);
        List<NotasACuenta> GetCompteCargaCtaAsoc(string ctaId);
        List<RespuestaDto> CompteCargaConfirma(CompteCargaConfirmaRequest request);
		List<CompteValorizaPendienteListaDto> ObtenerComprobantesPendientesDeValorizar(string ctaId);
        List<CompteValorizaDetalleRprListaDto> ObtenerComprobantesDetalleRpr(CompteValorizaRprDtosRequest request);
        List<CompteValorizaDtosListaDto> ObtenerComprobantesDtos(CompteValorizaRprDtosRequest request);
        List<CompteValorizaListaDto> ObtenerCompteValorizaLista(CompteValorizaRequest request);
        List<CuentaDatoDto> GetCuentaDatos(string cta, char tipo);

    }
}
