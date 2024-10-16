using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface IRemitoServicio : IServicio<Remito>
    {
        List<RemitoGenDto> ObtenerRemitosTransferidos(string admId);
        List<RespuestaDto> SeteaEstado(RSetearEstadoRequest request);
        List<RemitoVerConteoDto> VerConteos(string remCompte);
        List<RespuestaDto> ConfirmaRecepcion(RConfirmaRecepcionRequest reques);
	}
}
