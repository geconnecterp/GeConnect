using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.infraestructura.Core.Interfaces
{
    public interface IUriService
    {
        Uri GetPostPaginationUri(QueryFilters filter, string actionUrl);
        Uri GetPostPaginationUri(BusquedaProducto filter, string actionUrl);
        Uri GetPostPaginationUri(ConsultaMovFinancierosRequest filter, string actionUrl);

	}
}
