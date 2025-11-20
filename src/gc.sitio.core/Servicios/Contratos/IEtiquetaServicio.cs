using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IEtiquetaServicio
    {
        Task<RespuestaGenerica<CargaPreviaDto>> ObtenerCargaPrevia(string adm_id, string token);
        Task<RespuestaGenerica<IEDetalleDto>> ObtenerDetalleEtiquetas(QueryFilters filters, string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmarImpresionEtiqueta(ConfirmarEtiquetaRequestDto req, string token);
    }
}
