using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using System;

namespace gc.infraestructura.Core.Interfaces
{
    public interface IUriService
    {
        Uri GetPostPaginationUri(QueryFilters filter, string actionUrl);
        Uri GetPostPaginationUri(BusquedaProducto filter, string actionUrl);
        
    }
}
