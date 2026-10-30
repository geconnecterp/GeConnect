using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.Cajas
{
	public interface ICajaServicio
	{
		Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token);
		Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token);
		Task<List<CajaPVAbiertosDto>> ObtenerPVAbiertos(string admId, string token);

		Task<List<FaltanteDeCajaUltimaFechaDto>> FaltanteDeCajaUltimaFecha(string admId, string token);
		Task<List<FaltanteDeCajaDto>> BuscarFaltanteDeCaja(FaltanteDeCajaRequest request, string token);
		Task<List<FaltanteDeCajaDetalleDto>> BuscarFaltanteDeCajaDetalle(FaltanteDeCajaDetalleRequest request, string token);
		RespuestaGenerica<RespuestaDto> ConfirmarFaltanteDeCaja(ConfirmarFaltanteDeCajaRequest request, string token);
	}
}
