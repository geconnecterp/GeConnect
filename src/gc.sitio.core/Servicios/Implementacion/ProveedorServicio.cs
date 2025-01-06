using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ProveedorServicio : Servicio<ProveedorDto>, IProveedorServicio
    {
        private const string RutaAPI = "/api/apiproveedor";
		private const string ObtenerProveedorParaABM = "/GetProveedorParaABM";
		private readonly AppSettings _appSettings;

        public ProveedorServicio(IOptions<AppSettings> options,ILogger<ProveedorServicio> logger):base(options,logger,RutaAPI)
        {
            _appSettings = options.Value;
        }

		public List<ProveedorABMDto> GetProveedorParaABM(string ctaId, string token)
		{
			ApiResponse<List<ProveedorABMDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerProveedorParaABM}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorABMDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos del proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos del proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos del proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del proveedor.");
				throw;
			}
		}
	}
}
