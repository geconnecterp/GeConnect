using gc.infraestructura.Core.EntidadesComunes;
using System;

namespace gc.infraestructura.Core.Interfaces
{
    public interface IUriService
    {
        Uri GetPostPaginationUri(QueryFilters filter, string actionUrl);
    }
}
