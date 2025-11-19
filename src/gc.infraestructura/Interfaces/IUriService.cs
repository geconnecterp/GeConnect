using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.infraestructura.Core.Interfaces
{
    public interface IUriService
    {
        Uri GetPostPaginationUri(QueryFilters filter, string actionUrl);
        Uri GetPostPaginationUri(BusquedaProducto filter, string actionUrl);
        Uri GetPostPaginationUri(ConsultaMovFinancierosRequest filter, string actionUrl);
		Uri GetPostPaginationUri(ConsultaAnticipoFinanEmpRequest filter, string actionUrl);
		Uri GetPostPaginationUri(ConsultaLiqDeEmpleadoRequest filter, string actionUrl);
		Uri GetPostPaginationUri(ConsultarVencimientosRequest filter, string actionUrl);
		Uri GetPostPaginationUri(ConsultarCertificadosRequest filter, string actionUrl);
	}
}
