﻿using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class TipoComprobanteServicio : Servicio<TipoComprobanteDto>, ITipoComprobanteServicio
	{
		private const string RutaAPI = "/api/tipocomprobante";
		private const string ComptesPorCuentaBuscar = "/GetTipoComprobanteListaPorCuenta";
		private const string ComptesPorAfipBuscar = "/GetTipoComprobanteListaPorTipoAfip";
		private readonly AppSettings _appSettings;

		public TipoComprobanteServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		public async Task<List<TipoComprobanteDto>> BuscarTipoComprobanteListaPorTipoAfip(string afip_id, string token)
		{
			try
			{
				ApiResponse<List<TipoComprobanteDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ComptesPorAfipBuscar}?afip_id={afip_id}";
				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda afip_id:{afip_id}");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoComprobanteDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					return apiResponse.Data;
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					return [];
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"Algo no fue bien. Error interno {ex.Message}");
				return [];
			}
		}

		
		public async Task<List<TipoComprobanteDto>> BuscarTiposComptesPorCuenta(string cuenta, string token)
		{
			try
			{
				ApiResponse<List<TipoComprobanteDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ComptesPorCuentaBuscar}?cuenta={cuenta}";
				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda cuenta:{cuenta}");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoComprobanteDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					return apiResponse.Data;
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					return [];
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"Algo no fue bien. Error interno {ex.Message}");
				return [];
			}
		}
	}
}
