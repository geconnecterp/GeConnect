using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class DepositoServicio : Servicio<DepositoDto>, IDepositoServicio
    {
        private const string RutaAPI = "/api/apideposito";
        private const string Accion = "/GetDepositoXAdm";
        private readonly AppSettings _appSettings;

        public DepositoServicio(IOptions<AppSettings> options, ILogger<DepositoServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public List<DepositoDto> ObtenerDepositosDeAdministracion(string adm_id,string token )
        {
            ApiResponse<List<DepositoDto>>? respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{Accion}?adm_id={adm_id}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoDto>>>(stringData);
                    if (response == null)
                    {
                        throw new NegocioException("No se desserializo correctamente la lista de Depositos. Verifique");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los Depositos para el Combo: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los Depositos para el Combo");
                }

            }
            catch (NegocioException ex )
            {
                _logger.LogError(ex, "Error al intentar obtener Depositos para el Combo.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener Depositos para el Combo.");
                throw;
            }
        }
    }
}
