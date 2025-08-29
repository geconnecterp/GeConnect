using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;

namespace gc.api.core.Contratos.Servicios.Importacion
{
    public interface IApiImportarServicio
    {
        #region metodos de importacion

        List<MapeoColumnaDto> ObtenerPerfilDeProveedor(string ctaId);
        List<PrecioFileDatos> ObtenerPrecioFileDatos();
        List<RespuestaCPDto> CargarImportacionPrecioPerfil(AbmPlusGenDto req);
        RespuestaDto CargaPerfilCuenta(string ctaId, string usu, string adm, string json);
        #endregion

        #region Métodos de actualiacion
        List<ActualizaProveedorDto> ObtenerProveedoresConProductosParaActualizar();
        List<ProductoDetalleDto> ObtenerProductosDelProveedorParaActualizar(QueryFilters filters);
        RespuestaDto ConfirmarActualizacionPrecioProductosDeProveedor(AbmGenDto req);
        #endregion
    }
}
