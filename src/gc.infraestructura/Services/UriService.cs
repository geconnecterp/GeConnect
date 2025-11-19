

namespace gc.infraestructura.Core.Services
{
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
	using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
	using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
	using gc.infraestructura.Dtos.Financieros.Request;
	using gc.infraestructura.EntidadesComunes.Options;
    using System;

    public class UriService : IUriService
    {
        private readonly string _baseUri;

        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Uri GetPostPaginationUri(QueryFilters filter, string actionUrl)
        {
            string baseUrl = $"{_baseUri}{actionUrl}";
            return new Uri(baseUrl);
        }
        public Uri GetPostPaginationUri(BusquedaProducto filter, string actionUrl)
        {
            string baseUrl = $"{_baseUri}{actionUrl}";
            return new Uri(baseUrl);
        }
		public Uri GetPostPaginationUri(ConsultaMovFinancierosRequest filter, string actionUrl)
		{
			string baseUrl = $"{_baseUri}{actionUrl}";
			return new Uri(baseUrl);
		}

		public Uri GetPostPaginationUri(ConsultaAnticipoFinanEmpRequest filter, string actionUrl)
		{
			string baseUrl = $"{_baseUri}{actionUrl}";
			return new Uri(baseUrl);
		}

		public Uri GetPostPaginationUri(ConsultaLiqDeEmpleadoRequest filter, string actionUrl)
		{
			string baseUrl = $"{_baseUri}{actionUrl}";
			return new Uri(baseUrl);
		}
		public Uri GetPostPaginationUri(ConsultarVencimientosRequest filter, string actionUrl)
		{
			string baseUrl = $"{_baseUri}{actionUrl}";
			return new Uri(baseUrl);
		}
		public Uri GetPostPaginationUri(ConsultarCertificadosRequest filter, string actionUrl)
		{
			string baseUrl = $"{_baseUri}{actionUrl}";
			return new Uri(baseUrl);
		}
	}
}
