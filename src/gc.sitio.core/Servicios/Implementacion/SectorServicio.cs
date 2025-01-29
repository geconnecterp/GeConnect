using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
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
	public class SectorServicio : Servicio<SectorDto>, ISectorServicio
	{
		private const string RutaAPI = "/api/apisector";
		private const string ObtenerSectorParaABM = "/GetSectorParaABM";
		private const string ObtenerSubSectorParaABM = "/GetSubSectorParaABM";
		private const string ObtenerSubSector = "/GetSubSector";
		private const string ObtenerRubroParaABM = "/GetRubroParaABM";
		private const string ObtenerRubro = "/GetRubro";
		private readonly AppSettings _appSettings;
		public SectorServicio(IOptions<AppSettings> options, ILogger<SectorServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		public List<RubroListaABMDto> GetRubro(string rubId, string token)
		{
			ApiResponse<List<RubroListaABMDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerRubro}?rub_id={rubId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<RubroListaABMDto>>>(stringData);
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
					_logger.LogError($"Error al intentar obtener los datos de los rubros: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de los rubros");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del sector.");
				throw;
			}
		}

		public List<RubroListaABMDto> GetRubroParaABM(string secId, string token)
		{
			ApiResponse<List<RubroListaABMDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerRubroParaABM}?sec_id={secId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<RubroListaABMDto>>>(stringData);
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
					_logger.LogError($"Error al intentar obtener los datos de los rubros: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de los rubros");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del sector.");
				throw;
			}
		}

		public List<SectorDto> GetSectorParaABM(string secId, string token)
		{
			ApiResponse<List<SectorDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerSectorParaABM}?sec_id={secId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<SectorDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos del sector. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos del sector: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos del sector");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del sector.");
				throw;
			}
		}

		public List<SubSectorDto> GetSubSector(string rubgId, string token)
		{
			ApiResponse<List<SubSectorDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerSubSector}?rubg_id={rubgId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<SubSectorDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos del sub sector. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos del sector: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos del sub sector");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del sub sector.");
				throw;
			}
		}

		public List<SubSectorDto> GetSubSectorParaABM(string secId, string token)
		{
			ApiResponse<List<SubSectorDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerSubSectorParaABM}?sec_id={secId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<SubSectorDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos del sub sector. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos del sector: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos del sub sector");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del sub sector.");
				throw;
			}
		}
	}
}
