using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Seguridad;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ConfiguracionSeguridadServicio : IConfiguracionSeguridadServicio
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<ConfiguracionSeguridadServicio> _logger;

        public ConfiguracionSeguridadServicio(IOptions<AppSettings> options,
            ILogger<ConfiguracionSeguridadServicio> logger)
        {
            _appSettings = options.Value;
            _logger = logger;
        }

        public async Task<PoliticaClaveDto> ObtenerPoliticaClave(string token)
        {
            using var client = new HelperAPI().InicializaCliente(token);
            using var response = await client.GetAsync($"{_appSettings.RutaBase}/api/apitoken/politica-clave");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new NegocioException("No se pudo obtener la política de contraseñas.");

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<PoliticaClaveDto>>(body);
            return apiResponse?.Data
                ?? throw new NegocioException("La API no devolvió una política de contraseñas válida.");
        }

        public async Task<CambioClaveResultadoDto> CambiarClave(CambioClaveRequestDto request,
            string token, string? ip)
        {
            var helper = new HelperAPI();
            using var client = helper.InicializaCliente(request, token, out StringContent content);
            if (!string.IsNullOrWhiteSpace(ip))
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-ClientUsr", ip);

            using var response = await client.PostAsync($"{_appSettings.RutaBase}/api/apitoken/cambio-clave", content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("La API rechazó el cambio de contraseña con estado {StatusCode}.", response.StatusCode);
                return new CambioClaveResultadoDto
                {
                    resultado = 1,
                    resultado_id = "SOLICITUD_INVALIDA",
                    resultado_msj = string.IsNullOrWhiteSpace(body) ? "No se pudo procesar la solicitud." : body.Trim('"')
                };
            }

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CambioClaveResultadoDto>>(body);
            return apiResponse?.Data ?? new CambioClaveResultadoDto
            {
                resultado = -1,
                resultado_id = "SIN_RESPUESTA",
                resultado_msj = "La API no devolvió una respuesta válida."
            };
        }
    }
}
