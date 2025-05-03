using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class CuentaGastoServicio : Servicio<CuentaGastoDto>, ICuentaGastoServicio
	{
		private const string RutaAPI = "/api/apicuentagasto";
		private const string ObtenerCuentaGastoParaABM = "/GetCuentaGastoParaABM";
		private readonly AppSettings _appSettings;
		public CuentaGastoServicio(IOptions<AppSettings> options, ILogger<CuentaGastoServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		public List<CuentaGastoDto> GetCuentaDirectaParaABM(string ctagId, string token)
		{
			ApiResponse<List<CuentaGastoDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaGastoParaABM}?ctag_id={ctagId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaGastoDto>>>(stringData)
                            ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    }
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta directa. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta directa: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta directa");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta directa.");
				throw;
			}
		}
	}
}
