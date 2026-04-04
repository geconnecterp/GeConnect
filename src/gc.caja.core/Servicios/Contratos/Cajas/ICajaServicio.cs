using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface ICajaServicio
    {
        Task<CajaSettings> ObtenerAsync(string ruta);
        Task<RespuestaGenerica<RespuestaDto>> ValidarIntegridadUsuarioCaja(CajaValidaReqDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> AperturaCaja(CajaValidaReqDto req, string token);
    }
}
