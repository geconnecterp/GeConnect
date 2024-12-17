using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class RubroServicio : Servicio<RubroDto>,IRubroServicio
    {
        private const string RutaAPI = "/api/apirubro";
        private const string RubroLista = "/GetRubroLista";
        private readonly AppSettings _appSettings;

        public RubroServicio(IOptions<AppSettings> options, ILogger<RubroServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public List<RubroListaDto> ObtenerListaRubros(string token)
        {
            ApiResponse<List<RubroListaDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{RubroLista}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<List<RubroListaDto>>>(stringData);
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con los datos de los rubros. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los Rubros: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los Rubros");
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los Rubros.");
                throw;
            }
        }
    }
}
