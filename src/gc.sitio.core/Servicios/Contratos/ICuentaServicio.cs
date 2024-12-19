using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ICuentaServicio: IServicio<CuentaDto>
    {
        List<ProveedorListaDto> ObtenerListaProveedores(string token);
        Task<List<CuentaDto>> ObtenerListaCuentaComercial(string texto, char tipo, string token);
        Task<List<RPROrdenDeCompraDto>> ObtenerListaOCxCuenta(string cta_id, string token);
        Task<List<RPROrdenDeCompraDetalleDto>> ObtenerDetalleDeOC(string oc_compte, string token);
        List<ProveedorFamiliaListaDto> ObtenerListaProveedoresFamilia(string ctaId, string token);
		List<CuentaABMDto> GetCuentaParaABM(string ctaId, string token);
        List<CuentaFPDto> GetCuentaFormaDePago(string ctaId, string token);
        List<CuentaContactoDto> GetCuentaContactos(string cta_id, string token);
		List<CuentaObsDto> GetCuentaObs(string cta_id, string token);
		List<CuentaNotaDto> GetCuentaNota(string cta_id, string token);
		List<CuentaFPDto> GetFormaDePagoPorCuentaYFP(string cta_id, string fp_id, string token);
        List<CuentaContactoDto> GetCuentContactosporCuentaYTC(string cta_id, string tc_id, string token);
        List<CuentaNotaDto> GetCuentaNotaDatos(string cta_id, string usu_id, string token);
		List<CuentaObsDto> GetCuentaObsDatos(string cta_id, string to_id, string token);
	}
}
