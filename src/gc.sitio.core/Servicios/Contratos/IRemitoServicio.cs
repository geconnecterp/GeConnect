using gc.infraestructura.Dtos.Almacen.Rpr;
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
        Task<RespuestaDto> VerificaProductoEnRemito(string rm, string pId, string token);
        Task<RespuestaDto> RTRCargarConteos(List<ProductoGenDto> lista,bool esModificacion, string token);
        Task<List<RTRxULDto>> RTRCargarConteosXUL(string reCompte, string token);

	}
}
