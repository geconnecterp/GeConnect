using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;

namespace gc.sitio.core.Servicios.Contratos.Importacion
{
    public interface IImportarServicio
    {
        Task<RespuestaGenerica<RespuestaCPDto>> CargarImportacionPrecio(AbmPlusGenDto request, string tokenCookie);
        Task<RespuestaGenerica<MapeoColumnaDto>> ObtenerPerfilDeProveedor(string ctaId, string tokenCookie);
        Task<RespuestaGenerica<PrecioFileDatos>> ObtenerPrecioFileDatos(string token);
    }
}
