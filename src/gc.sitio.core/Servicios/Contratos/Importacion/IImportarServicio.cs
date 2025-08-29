using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;

namespace gc.sitio.core.Servicios.Contratos.Importacion
{
    public interface IImportarServicio
    {
        #region IMPORTACIÓN

       
        Task<RespuestaGenerica<RespuestaCPDto>> CargarImportacionPrecio(AbmPlusGenDto request, string tokenCookie);
        Task<RespuestaGenerica<MapeoColumnaDto>> ObtenerPerfilDeProveedor(string ctaId, string tokenCookie);
        Task<RespuestaGenerica<PrecioFileDatos>> ObtenerPrecioFileDatos(string token);

        #endregion

        #region Métodos de actualiacion
        Task<RespuestaGenerica<ActualizaProveedorDto>> ObtenerProveedoresConProductosParaActualizar(string tokenCookie);
        Task<RespuestaGenerica<ProductoDetalleDto>> ObtenerProductosDelProveedorParaActualizar(QueryFilters filters, string tokenCookie);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmarActualizacionPrecioProductosDeProveedor(AbmGenDto req, string tokenCookie);
        #endregion
    }
}
