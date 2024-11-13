using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Deposito;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private const string AccionInfoBox = "/GetDepositoInfoBox";
        private const string AccionInfoStk = "/GetDepositoInfoStk";
        private const string AccionInfoStkVal = "/GetDepositoInfoStkVal";

		private const string RTI_REMITOS_PENDIENTES = "/RemitosPendientes";

		private const string RutaApiAlmacen = "/api/apialmacen";
		private const string BOX_BUSCAR_POR_DEPOSITO = "/ObtenerListaDeBoxesPorDeposito";
		private const string BOX_BUSCAR = "/ObtenerInfoDeBox";
		private readonly AppSettings _appSettings;

        public DepositoServicio(IOptions<AppSettings> options, ILogger<DepositoServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public async Task<List<DepositoInfoBoxDto>> BuscarBoxLibres(string depo_id, bool soloLibres, string token)
        {
            ApiResponse<List<DepositoInfoBoxDto>>? respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{AccionInfoBox}?depo_id={depo_id}&soloLibre={soloLibres}";
                response = await client.GetAsync(link);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoInfoBoxDto>>>(stringData);
                    if (response == null)
                    {
                        throw new NegocioException("No se desserializo correctamente la lista de Box. Verifique");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los BOX del Depósito: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los BOX del Depósito");
                }

            }
            catch (NegocioException ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los BOX del Depósito.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los BOX del Depósito.");
                throw;
            }
        }

        public async Task<List<DepositoInfoStkDto>> BuscarDepositoInfoStk(string depo_id,  string token)
        {
            ApiResponse<List<DepositoInfoStkDto>>? respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{AccionInfoStk}?depo_id={depo_id}";
                response = await client.GetAsync(link);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoInfoStkDto>>>(stringData);
                    if (response == null)
                    {
                        throw new NegocioException("No se desserializo correctamente la lista de Box. Verifique");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los productos del Depósito: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los productos del Depósito");
                }

            }
            catch (NegocioException ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los productos del Depósito.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los productos del Depósito.");
                throw;
            }
        }

        public async Task<List<DepositoInfoStkValDto>> BuscarDepositoInfoStkVal(string adm_id,string depo_id,string concepto, string token)
        {
            ApiResponse<List<DepositoInfoStkValDto>>? respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{AccionInfoStkVal}?adm_id={adm_id}&depo_id={depo_id}&concepto={concepto}";
                response = await client.GetAsync(link);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoInfoStkValDto>>>(stringData);
                    if (response == null)
                    {
                        throw new NegocioException("No se desserializo correctamente la lista de Stk Valorizado. Verifique");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener la lista de Stk Valorizado: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener la lista de Stk Valorizado");
                }

            }
            catch (NegocioException ex)
            {
                _logger.LogError(ex, "Error al intentar obtener la lista de Stk Valorizado.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener la lista de Stk Valorizado.");
                throw;
            }
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

        public async Task<List<RemitoGenDto>> ObtenerRemitos(string admId, string token)
        {
            ApiResponse<List<RemitoGenDto>>? respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{RTI_REMITOS_PENDIENTES}?adm_id={admId}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<RemitoGenDto>>>(stringData);
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
            catch (NegocioException ex)
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

		public async Task<List<DepositoInfoBoxDto>> BuscarBoxPorDeposito(string depoId, string token)
		{
			ApiResponse<List<DepositoInfoBoxDto>>? respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response = null;
				var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{BOX_BUSCAR_POR_DEPOSITO}?depoId={depoId}";
				response = await client.GetAsync(link);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoInfoBoxDto>>>(stringData);
					if (response == null)
					{
						throw new NegocioException("No se desserializo correctamente la lista de Box. Verifique");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los BOX del Depósito: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los BOX del Depósito");
				}

			}
			catch (NegocioException ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los BOX del Depósito.");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los BOX del Depósito.");
				throw;
			}
		}

		public async Task<List<DepositoInfoBoxDto>> ObtenerInfoDeBox(string boxId, string token)
		{
			ApiResponse<List<DepositoInfoBoxDto>>? respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response = null;
				var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{BOX_BUSCAR}?boxId={boxId}";
				response = await client.GetAsync(link);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					respuesta = JsonConvert.DeserializeObject<ApiResponse<List<DepositoInfoBoxDto>>>(stringData);
					if (response == null)
					{
						throw new NegocioException("No se desserializo correctamente la lista de Box. Verifique");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener la información del BOX: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener la información del BOX");
				}

			}
			catch (NegocioException ex)
			{
				_logger.LogError(ex, "Error al intentar obtener la información del BOX.");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener la información del BOX.");
				throw;
			}
		}
	}
}
