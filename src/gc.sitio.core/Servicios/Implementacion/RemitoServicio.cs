using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class RemitoServicio : Servicio<RemitoDto>, IRemitoServicio
    {
        private const string RutaAPI = "/api/apiremito";
        private const string RemitosTransferidosLista = "/ObtenerRemitosTransferidosLista";
        private const string RemitosSeteaEstado = "/SetearEstado";
        private const string RemitosVerConteos = "/VerConteos";
        private const string RemitosConfirmarRecepcion = "/ConfirmarRecepcion";
        private const string Verif_ProductoEnRemito = "/VerificaProductoEnRemito";
        private const string RemitoCargarConteos = "/RTRCargarConteos";
        private const string RemitosCargarConteosXUL = "/RTRCargarConteosXUL";
		private const string RemitoExternoProductosAValidar = "/cargar-productos-desde-comprobante";
		private const string RemitoExternoConfirmar = "/confirmar-remito-externo";

		private readonly AppSettings _appSettings;
        public RemitoServicio(IOptions<AppSettings> options, ILogger<RemitoServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public async Task<List<RespuestaDto>> ConfirmarRecepcion(string remCompte, string usuario, string token)
        {
            ApiResponse<List<RespuestaDto>> apiResponse;

            HelperAPI helper = new();
            RConfirmaRecepcionRequest request = new() { remCompte = remCompte, usuario = usuario };
            HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosConfirmarRecepcion}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API devolvió error. Parametros rem_compte:{remCompte}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<RemitoGenDto>> ObtenerRemitosTransferidos(string admId, string token)
        {
            ApiResponse<List<RemitoGenDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosTransferidosLista}?admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RemitoGenDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<RespuestaDto>> SetearEstado(string remCompte, string estado, string token)
        {
            ApiResponse<List<RespuestaDto>> apiResponse;

            HelperAPI helper = new HelperAPI();
            RSetearEstadoRequest request = new() { remCompte = remCompte, estado = estado };
            HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosSeteaEstado}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API devolvió error. Parametros rem_compte:{remCompte}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<RemitoVerConteoDto>> VerConteos(string remCompte, string token)
        {
            ApiResponse<List<RemitoVerConteoDto>> apiResponse;

            HelperAPI helper = new();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosVerConteos}?remCompte={remCompte}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda remCompte:{remCompte}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RemitoVerConteoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<RespuestaDto> VerificaProductoEnRemito(string rm, string pId, string token)
        {
            try
            {
                ApiResponse<RespuestaDto> apiResponse;

                HelperAPI helper = new HelperAPI();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{Verif_ProductoEnRemito}?remCompte={rm}&pId={pId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda remCompte={rm}&pId={pId} - stringData{stringData}");
                        return new() { resultado = -1, resultado_msj = "Se produjo un problema al querer verifiar el producto en el Remito. Consultar el log." }; ;
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return apiResponse.Data;
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { resultado = -1, resultado_msj = "Se produjo un problema al querer verifiar el producto en el Remito. Consultar el log para conocer la posible razón del error." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar producto en Remito");
                return new() { resultado_msj = "Algo no fue bien al intentar verificar el producto en el remito. Verifique Log.", resultado = -1 };
            }
        }

        public async Task<RespuestaDto> RTRCargarConteos(List<ProductoGenDto> lista,bool esModificacion,string token)
        {
            try
            {
                ApiResponse<RespuestaDto> apiResponse;

                HelperAPI helper = new HelperAPI();
                var req = new CargarJsonGenRequest();
                var json = JsonConvert.SerializeObject(lista);
                req.json_str = json;

                HttpClient client = helper.InicializaCliente(req, token,out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitoCargarConteos}?esMod={esModificacion}";

                response = await client.PostAsync(link,contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"La API no devolvió dato alguno. RTRCargarConteos.");
                        return new() { resultado = -1, resultado_msj = "Se produjo un problema al querer realizar la carga de conteos (RTR). Consultar el log." }; ;
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                    return apiResponse.Data;
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { resultado = -1, resultado_msj = "Se produjo un problema al querer verifiar el producto en el Remito. Consultar el log para conocer la posible razón del error." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al Cargar Conteos (RTR)");
                return new() { resultado_msj = "Algo no fue bien al intentar Cargar el Conteo de productos. Verifique Log.", resultado = -1 };
            }
        }

		public async Task<List<RTRxULDto>> RTRCargarConteosXUL(string reCompte, string token)
		{
			ApiResponse<List<RTRxULDto>> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosCargarConteosXUL}?reCompte={reCompte}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda reCompte:{reCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RTRxULDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<RemitoExternoValidaDto>> CargarProductosDesdeComprobante(RemitoExternoValidaRequest request, string token)
		{
			try
			{
				ApiResponse<List<RemitoExternoValidaDto>>? apiResponse;
				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitoExternoProductosAValidar}";

				response = await client.PostAsync(link, contentData);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RemitoExternoValidaDto>>>(stringData);

					var listado = apiResponse?.Data;

					return new RespuestaGenerica<RemitoExternoValidaDto> { Ok = true, ListaEntidad = listado };

				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
					if (error.TypeException.Equals(nameof(NegocioException)))
					{
						throw new NegocioException(error.Detail);
					}
					else if (error.TypeException.Equals(nameof(NotFoundException)))
					{
						throw new NegocioException(error.Detail);
					}
					else
					{
						throw new Exception(error.Detail);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

				throw new Exception("Algo no fue bien al intentar cargar los productos desde el comprobante.");
			}
		}

		public RespuestaGenerica<RespuestaDto> ConfirmarRemitoExterno(ConfirmarRemitoExternoRequest request, string token)
		{
			try
			{
				ApiResponse<List<RespuestaDto>> apiResponse;

				HelperAPI helper = new HelperAPI();

				HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitoExternoConfirmar}";

				response = client.PostAsync(link, contentData).Result;

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API devolvió error.");
						return new();
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
					return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data.First() };
				}
				else
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					return new();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al confirmar el remito externo");
				return new() { Mensaje = "Algo no fue bien al intentar la confirmación del remito externo. Verifique Log.", EsError = true };
			}
		}
	}
}
