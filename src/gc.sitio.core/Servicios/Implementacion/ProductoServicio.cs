﻿using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.Intrinsics.Arm;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ProductoServicio : Servicio<ProductoDto>, IProductoServicio
    {
        private const string RutaAPI = "/api/apiproducto";
        private const string BUSCAR_PROD = "/ProductoBuscar";
		private const string BUSCAR_PROD_POR_IDS = "/ProductoBuscarPorIds";
		private const string BUSCAR_LISTA = "/ProductoListaBuscar";
        private const string INFOPROD_STKD = "/InfoProductoStkD";
        private const string INFOPROD_StkBoxes = "/InfoProductoStkBoxes";
        private const string INFOPROD_STKA = "/InfoProductoStkA";
        private const string INFOPROD_MovStk = "/InfoProductoMovStk";
        private const string INFOPROD_LP = "/InfoProductoLP";

        private const string RPRAUTOPEND = "/RPRAutorizacionPendiente";
        private const string RPRCOMPTESPEND = "/RPRObtenerAutoComptesPendientes";
        private const string RPRREGISTRAR = "/RPRRegistrar";
		private const string RPRCARGAR = "/RPRCargar";
		private const string RPRELIMINA = "/RPRElimina";
		private const string RPRCONFIRMA = "/RPRConfirma";
		private const string RPROBTENERJSON = "/RPRObtenerJson";
		private const string RPROBTENERDATOSVERCOMPTE = "/RPRObtenerItemVerCompte";
        private const string RPROBTENERDATOSVERCONTEOS = "/RPRObtenerVerConteos";

		//almacena Box 
		private const string RutaApiAlmacen = "/api/apialmacen";
        private const string RPR_AB_VALIDA_UL = "/ValidarUL";
        private const string RPR_AB_VALIDA_BOX = "/ValidarBox";
        private const string RPR_AB_ALMACENA_BOX = "/AlmacenaBoxUl";

        private const string TI_ListaBox = "/GetTIListaBox";
        private const string TI_ListaRubro = "/GetTIListaRubro";
        private const string TI_ListaProductos = "/BuscaTIListaProductos";
        


        //Transferencia Interna
        private const string TR_AU_PENDIENTE = "/TRAutorizacionPendiente";

        private readonly AppSettings _appSettings;

        public ProductoServicio(IOptions<AppSettings> options, ILogger<ProductoServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public async Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token)
        {
            ApiResponse<ProductoBusquedaDto> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            string parametros = EvaluarEntidad4Link(busqueda);
            var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PROD}?{parametros}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
                    return new ProductoBusquedaDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBusquedaDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new ProductoBusquedaDto();

            }
        }

		public async Task<List<ProductoBusquedaDto>> BusquedaBaseProductosPorIds(BusquedaBase busqueda, string token)
		{
			ApiResponse<List<ProductoBusquedaDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;
			string parametros = EvaluarEntidad4Link(busqueda);
			var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PROD_POR_IDS}?{parametros}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
					return new List<ProductoBusquedaDto>();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoBusquedaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new List<ProductoBusquedaDto>();

			}
		}

		public async Task<List<ProductoListaDto>> BusquedaListaProductos(BusquedaProducto busqueda, string token)
        {
            ApiResponse<List<ProductoListaDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            string parametros = EvaluarEntidad4Link(busqueda);
            var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_LISTA}?{parametros}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoListaDto>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkD>> InfoProductoStkD(string id, string admId, string token)
        {
            ApiResponse<List<InfoProdStkD>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_STKD}?id={id}&admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkD>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkBox>> InfoProductoStkBoxes(string id, string adm, string depo, string token)
        {
            ApiResponse<List<InfoProdStkBox>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_StkBoxes}?id={id}&adm={adm}&depo={depo}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkBox>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkA>> InfoProductoStkA(string id, string admId, string token)
        {
            ApiResponse<List<InfoProdStkA>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_STKA}?id={id}&admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkA>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdMovStk>> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta, string token)
        {
            ApiResponse<List<InfoProdMovStk>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_MovStk}?id={id}&adm={adm}&depo={depo}&tmov={tmov}&desde={desde}&hasta={hasta}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}-tmov:{tmov}-desde:{desde}-hasta:{hasta}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdMovStk>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdLP>> InfoProductoLP(string id, string token)
        {
            ApiResponse<List<InfoProdLP>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_LP}?id={id}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdLP>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<RPRAutorizacionPendienteDto>> RPRObtenerAutorizacionPendiente(string adm, string token)
        {
            ApiResponse<List<RPRAutorizacionPendienteDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRAUTOPEND}?adm={adm}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda adm:{adm}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRAutorizacionPendienteDto>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<RPRRegistroResponseDto> RPRRegistrarProductos(List<RPRProcuctoDto> prods, string admId, string ul, string token)
        {
            ApiResponse<RPRRegistroResponseDto> apiResponse;

            HelperAPI helper = new HelperAPI();

            foreach (var item in prods)
            {
                item.ul_id = ul;
            }

            HttpClient client = helper.InicializaCliente(prods, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRREGISTRAR}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. ");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<RPRRegistroResponseDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<RPRAutoComptesPendientesDto>> RPRObtenerComptesPendiente(string adm, string token)
        {
            ApiResponse<List<RPRAutoComptesPendientesDto>> apiResponse;

            HelperAPI helper = new();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRCOMPTESPEND}?adm_id={adm}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda adm:{adm}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRAutoComptesPendientesDto>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

		public async Task<List<RespuestaDto>> RPRCargarCompte(string json_str, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
            RPRCargarRequest request = new() { json_str = json_str };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRCARGAR}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros json_str:{json_str}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> RPREliminarCompte(string rp, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRELIMINA}?rp={rp}";
			response = await client.DeleteAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> RPRConfirmarRPR(string rp, string adm_id, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new HelperAPI();
			RPRAConfirmarRequest request = new() { rp = rp, adm_id = adm_id };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRCONFIRMA}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<JsonDto>> RPObtenerJsonDesdeRP(string rp, string token)
		{
			ApiResponse<List<JsonDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPROBTENERJSON}?rp={rp}";
			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<JsonDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RPRItemVerCompteDto>> RPRObtenerItemVerCompte(string rp, string token)
		{
			ApiResponse<List<RPRItemVerCompteDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPROBTENERDATOSVERCOMPTE}?rp={rp}";
			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRItemVerCompteDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RPRVerConteoDto>> RPRObtenerItemVerConteos(string rp, string token)
		{
			ApiResponse<List<RPRVerConteoDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPROBTENERDATOSVERCONTEOS}?rp={rp}";
			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRVerConteoDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RprResponseDto> ValidarUL(string ul, string adm, string token)
        {
            ApiResponse<RprResponseDto> apiResponse;

            HelperAPI helper = new HelperAPI();
            RprABRequest request = new() { UL = ul, AdmId = adm };
            HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{RPR_AB_VALIDA_UL}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(request)}");
                    return new RprResponseDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<RprResponseDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
                }
                catch
                {
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = stringData };
                }

            }
        }

        public async Task<RprResponseDto> ValidarBox(string box, string adm, string token)
        {
            ApiResponse<RprResponseDto> apiResponse;

            HelperAPI helper = new HelperAPI();
            RprABRequest request = new() { Box = box, AdmId = adm };
            HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{RPR_AB_VALIDA_BOX}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(request)}");
                    return new RprResponseDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<RprResponseDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
                }
                catch
                {
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = stringData };
                }

            }
        }

        public async Task<RprResponseDto> ConfirmaBoxUl(string box, string ul, string adm, string token)
        {
            ApiResponse<RprResponseDto> apiResponse;

            HelperAPI helper = new HelperAPI();
            RprABRequest request = new() { Box = box, UL = ul, AdmId = adm };
            HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{RPR_AB_ALMACENA_BOX}";

            response = await client.PostAsync(link, contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(request)}");
                    return new RprResponseDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<RprResponseDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
                }
                catch
                {
                    return new RprResponseDto() { Resultado = -1, Resultado_msj = stringData };
                }
            }
        }

        public async Task<List<AutorizacionTIDto>> TRObtenerAutorizacionesPendientes(string admId, string usuId, string titId, string token)
        {
            ApiResponse<List<AutorizacionTIDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{TR_AU_PENDIENTE}?admId={admId}&usuId={usuId}&titId={titId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AutorizacionTIDto>>>(stringData)?? new ApiResponse<List<AutorizacionTIDto>>(new List<AutorizacionTIDto>());
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new();
                }
                catch
                {
                    return new();
                }
            }
        }

        public async Task<List<BoxRubProductoDto>> PresentarBoxDeProductos(string tr, string admId, string usuId, string token)
        {
            ApiResponse<List<BoxRubProductoDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{TI_ListaBox}?tr={tr}&admId={admId}&usuId={usuId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxRubProductoDto>>>(stringData) ?? new ApiResponse<List<BoxRubProductoDto>>(new List<BoxRubProductoDto>());
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new();
                }
                catch
                {
                    return new();
                }
            }
        }

        public async Task<List<BoxRubProductoDto>> PresentarRubrosDeProductos(string tr, string admId, string usuId, string token)
        {
            ApiResponse<List<BoxRubProductoDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{TI_ListaRubro}?tr={tr}&admId={admId}&usuId={usuId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxRubProductoDto>>>(stringData) ?? new ApiResponse<List<BoxRubProductoDto>>(new List<BoxRubProductoDto>());
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new();
                }
                catch
                {
                    return new();
                }
            }
        }

        public async Task<List<TiListaProductoDto>> BuscaTIListaProductos(string tr, string admId, string usuId, string? boxid, string? rubId, string token)
        {
            ApiResponse<List<TiListaProductoDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaApiAlmacen}{TI_ListaProductos}?tr={tr}&admId={admId}&usuId={usuId}&boxid={boxid}&rubroid={rubId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TiListaProductoDto>>>(stringData) ?? new ApiResponse<List<TiListaProductoDto>>(new List<TiListaProductoDto>());
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {
                    var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    return new();
                }
                catch
                {
                    return new();
                }
            }
        }
    }
}
