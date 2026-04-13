using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.Cajas
{
	public interface ICajaServicio
	{
		Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token);
		Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token);
		Task<List<CajaPVAbiertosDto>> ObtenerPVAbiertos(string admId, string token);
	}
}
