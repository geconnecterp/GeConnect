using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IRemitoServicio : IServicio<RemitoDto>
    {
        Task<List<RemitoGenDto>> ObtenerRemitosTransferidos(string admId, string token);
        Task<List<RespuestaDto>> SetearEstado(string remCompte, string estado, string token);
        Task<List<RemitoVerConteoDto>> VerConteos(string remCompte, string token);
        Task<List<RespuestaDto>> ConfirmarRecepcion(string remCompte, string usuario, string token);
	}
}
