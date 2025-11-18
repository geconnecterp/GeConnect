using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.Etiqueta;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiEtiquetaServicio
    {
        List<CargaPreviaDto> ObtenerCargaPreviaUsuario(string adm_id);
        List<IEDetalleDto> ObtenerDetalleEtiquetas(QueryFilters filters);
    }
}
