using gc.infraestructura.Core.EntidadesComunes.Options;
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
	public class VendedorServicio : Servicio<VendedorDto>, IVendedorServicio
	{
		private const string RutaAPI = "/api/tiposvs";
		private const string VendedoresLista = "/GetVendedoresLista";
		private readonly AppSettings _appSettings;
		public VendedorServicio(IOptions<AppSettings> options, ILogger<VendedorServicio> logger) : base(options, logger, RutaAPI)
		{
			_appSettings = options.Value;
		}

		public List<VendedorDto> GetVendedorLista(string token)
		{
			ApiResponse<List<VendedorDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{VendedoresLista}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<VendedorDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de los vendedores. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los vendedores: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los vendedores");
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
