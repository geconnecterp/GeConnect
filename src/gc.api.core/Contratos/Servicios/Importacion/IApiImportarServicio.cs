using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;

namespace gc.api.core.Contratos.Servicios.Importacion
{
    public interface IApiImportarServicio
    {
        List<MapeoColumnaDto> ObtenerPerfilDeProveedor(string ctaId);
        List<PrecioFileDatos> ObtenerPrecioFileDatos();
        List<RespuestaCPDto> CargarImportacionPrecioPerfil(AbmPlusGenDto req);
        RespuestaDto CargaPerfilCuenta(string ctaId, string usu, string adm, string json);
    }
}
