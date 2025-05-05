using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Response;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.General;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using System.Security.Cryptography;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request;
using System.Diagnostics;
using gc.infraestructura.Core.Exceptions;
using System.Security.Claims;
using System;

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
		private const string INFOPROD_TM = "/ObtenerTiposMotivo";
		private const string INFOPROD_IE_MES = "/InfoProdIExMes";
		private const string INFOPROD_IE_SEMANA = "/InfoProdIExSemana";
		private const string INFOPROD_SUSTITUTO = "/InfoProdSustituto";
		private const string INFOPROD = "/InfoProd";
		private const string TIPO_DE_AJUSTE = "/ObtenerTipoDeAjusteDeStock";
		private const string AJUSTE_PREVIO_CARGADO = "/ObtenerAJPreviosCargados";
		private const string AJUSTE_REVERTIDO = "/ObtenerAJREVERTIDO";
		private const string AJUSTE_CONFIRMAR = "/ConfirmarAjusteStk";

		private const string DEVOLUCION_PREVIO_CARGADO = "/ObtenerDPPreviosCargados";
		private const string DEVOLUCION_REVERTIDO = "/ObtenerDPREVERTIDO";
		private const string DEVOLUCION_CONFIRMAR = "/ConfirmarDP";

		private const string RPRAUTOPEND = "/RPRAutorizacionPendiente";
		private const string RPRCOMPTESPEND = "/RPRObtenerAutoComptesPendientes";
		private const string RPRREGISTRAR = "/RPRRegistrar";
		private const string RPRCARGAR = "/RPRCargar";
		private const string RPRELIMINA = "/RPRElimina";
		private const string RPRCONFIRMA = "/RPRConfirma";
		private const string RPROBTENERJSON = "/RPRObtenerJson";
		private const string RPROBTENERDATOSVERCOMPTE = "/RPRObtenerItemVerCompte";
		private const string RPROBTENERDATOSVERCONTEOS = "/RPRObtenerVerConteos";
		private const string RPRxULOBTENER = "/RPRxUL";
		private const string RPRxULDETALLEOBTENER = "/RPRxULDetalle";
		private const string PRODUCTOS_POR_FAMILIA = "/GetProductosPorFamilia";

		//almacena Box 
		private const string RutaApiAlmacen = "/api/apialmacen";
		private const string RPR_AB_VALIDA_UL = "/ValidarUL";
		private const string RPR_AB_VALIDA_BOX = "/ValidarBox";
		private const string RPR_AB_ALMACENA_BOX = "/AlmacenaBoxUl";

		private const string TI_ListaBox = "/GetTIListaBox";
		private const string TI_ListaRubro = "/GetTIListaRubro";
		private const string TI_ListaProductos = "/BuscaTIListaProductos";
		private const string TI_RESGUARDA_PROD_CARRITO = "/ResguardarProductoCarrito";
		private const string TI_VALIDA_PROD_CARRITO = "/ValidaProductoCarrito";
		private const string TI_CONTROL_SALIDA = "/ControlSalidaTI";
		private const string TI_VALIDA_PENDIENTE = "/TRValidaPendiente";
		private const string TI_CONFIRMA = "/TR_Confirma";
		private const string TI_NUEVA_SIN_AUTO = "/TRNuevaSinAuto";
		private const string TI_VER_CTRL_SALIDA = "/TRVerCtrlSalida";
		private const string TI_CARGAR_CTRL_SALIDA = "/TRCargarCtrlSalida";



		//Transferencia Interna
		private const string TR_AU_PENDIENTE = "/TRAutorizacionPendiente";
		private const string TR_PENDIENTES = "/ObtenerTRPendientes";
		private const string TR_AUT_SUCURSALES = "/ObtenerTRAutSucursales";
		private const string TR_AUT_PI = "/ObtenerTRAutPI";
		private const string TR_AUT_PI_DETALLE = "/ObtenerTRAutPIDetalle";
		private const string TR_AUT_Depositos = "/ObtenerTRAutDepositos";
		private const string TR_AUT_Analiza = "/TRAutAnaliza";
		private const string TR_Busca_Vto = "/BuscarFechaVto";
		private const string TR_AUT_Sustituto = "/TRObtenerSustituto";
		private const string TR_AUT_Confirma_Auto = "/TRConfirmaAutorizaciones";
		private const string TR_Ver_Conteos = "/TRVerConteos";
		private const string TR_Validar_Transferencia = "/TRValidarTransferencia";



		//NCYPI
		private const string OC_Productos = "/NCPICargarListaDeProductos";
		private const string OC_Productos_Pag = "/NCPICargarListaDeProductosPag";
		private const string OC_Productos_Pag2 = "/NCPICargarListaDeProductosPag2";
		private const string OC_Cargar_Pedido = "/NCPICargaPedido";
		private const string OC_Cargar_Detalle = "/CargarProductosDeOC";
		private const string OC_Tope = "/CargarTopesDeOC";
		private const string OC_Resumen = "/CargarResumenDeOC";
		private const string OC_Confirmar = "/ConfirmarOC";
		private const string OC_Lista = "/CargarOrdenDeCompraConsultaLista";
		private const string OC_Detalle = "/CargarDetalleDeOC";
		private const string OC_Rpr_Asociada = "/CargarRprAsociadaDeOC";
		private const string OC_Modificar = "/ModificarOC";
		private const string OC_ObtenerPorOcCompte = "/ObtenerOrdenDeCompraPorOcCompte";

		private const string OC_Cargar_Lista = "/CargarOrdenesDeCompraList";

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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBusquedaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoBusquedaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new List<ProductoBusquedaDto>();

			}
		}

		public async Task<(List<ProductoListaDto>, MetadataGrid?)> BusquedaListaProductos(BusquedaProducto busqueda, string token)
		{
			ApiResponse<List<ProductoListaDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(busqueda, token, out StringContent content);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_LISTA}";

			response = await client.PostAsync(link, content);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoListaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return (apiResponse.Data ?? [], apiResponse.Meta ?? new()	);
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return (new(), null);
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkD>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos")	;
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<InfoProdStkBox>> InfoProductoStkBoxes(string id, string adm, string depo, string token, string box = "")
		{
			ApiResponse<List<InfoProdStkBox>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			if (string.IsNullOrEmpty(box) || string.IsNullOrWhiteSpace(box))
			{
				box = "%";
			}

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_StkBoxes}?id={id}&adm={adm}&depo={depo}&box={box}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkBox>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkA>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

			//if (depo.Equals("%"))
			//{
			//    depo = "porc.";
			//}
			//if (tmov.Equals("%"))
			//{
			//    tmov = "porc.";
			//}

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_MovStk}?id={id}&adm={adm}&depo={depo}&tmov={tmov}&desde={desde.Ticks}&hasta={hasta.Ticks}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}-tmov:{tmov}-desde:{desde}-hasta:{hasta}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdMovStk>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdLP>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<NDeCYPI.InfoProdIExMesDto>> InfoProdIExMes(string admId, string pId, int meses, string token)
		{
			ApiResponse<List<NDeCYPI.InfoProdIExMesDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_IE_MES}?admId={admId}&pId={pId}&meses={meses}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda admId:{admId} pId:{pId} meses:{meses}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<NDeCYPI.InfoProdIExMesDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<NDeCYPI.InfoProdIExSemanaDto>> InfoProdIExSemana(string admId, string pId, int semanas, string token)
		{
			ApiResponse<List<NDeCYPI.InfoProdIExSemanaDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_IE_SEMANA}?admId={admId}&pId={pId}&semanas={semanas}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda admId:{admId} pId:{pId} semanas:{semanas}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<NDeCYPI.InfoProdIExSemanaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<ProductoNCPISustitutoDto>> InfoProdSustituto(string pId, string tipo, string admId, bool soloProv, string token)
		{
			ApiResponse<List<ProductoNCPISustitutoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_SUSTITUTO}?pId={pId}&tipo={tipo}&admId={admId}&soloProv={soloProv}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda admId:{admId} pId:{pId} tipo:{tipo} soloProv:{soloProv}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoNCPISustitutoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<NDeCYPI.InfoProductoDto>> InfoProd(string pId, string token)
		{
			ApiResponse<List<NDeCYPI.InfoProductoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD}?pId={pId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda pId:{pId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<NDeCYPI.InfoProductoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TipoAjusteDeStockDto>> ObtenerTipoDeAjusteDeStock(string token)
		{
			ApiResponse<List<TipoAjusteDeStockDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TIPO_DE_AJUSTE}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoAjusteDeStockDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<AjustePrevioCargadoDto>> ObtenerAJPreviosCargados(string admId, string token)
		{
			ApiResponse<List<AjustePrevioCargadoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{AJUSTE_PREVIO_CARGADO}?admId={admId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AjustePrevioCargadoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos")	;
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<AjusteRevertidoDto>> ObtenerAJREVERTIDO(string ajId, string token)
		{
			ApiResponse<List<AjusteRevertidoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{AJUSTE_REVERTIDO}?ajId={ajId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AjusteRevertidoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> ConfirmarAjusteStk(string json, string admId, string usuId, string compteOri, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			ConfirmarAjusteStkRequest request = new() { json = json, admId = admId, usuId = usuId, compteOri = compteOri };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{AJUSTE_CONFIRMAR}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros json:{json}");
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

		public async Task<List<DevolucionPrevioCargadoDto>> ObtenerDPPreviosCargados(string admId, string ctaId, string token)
		{
			ApiResponse<List<DevolucionPrevioCargadoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{DEVOLUCION_PREVIO_CARGADO}?admId={admId}&ctaId={ctaId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DevolucionPrevioCargadoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<DevolucionRevertidoDto>> ObtenerDPREVERTIDO(string dvCompte, string token)
		{
			ApiResponse<List<DevolucionRevertidoDto>> apiResponse;

			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{DEVOLUCION_REVERTIDO}?dvCompte={dvCompte}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DevolucionRevertidoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> ConfirmarDP(string json, string admId, string usuId, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			ConfirmarDPRequest request = new() { json = json, admId = admId, usuId = usuId };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{DEVOLUCION_CONFIRMAR}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros json:{json}");
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

		public async Task<List<AutorizacionPendienteDto>> RPRObtenerAutorizacionPendiente(string adm, string token)
		{
			ApiResponse<List<AutorizacionPendienteDto>> apiResponse;

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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AutorizacionPendienteDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RegistroResponseDto> RPRRegistrarProductos(List<ProductoGenDto> prods, string admId, string ul, bool esModificacion, string token)
		{
			ApiResponse<RegistroResponseDto> apiResponse;

			HelperAPI helper = new HelperAPI();

			foreach (var item in prods)
			{
				item.ul_id = ul;
			}

			HttpClient client = helper.InicializaCliente(prods, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRREGISTRAR}?esMod={esModificacion}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. ");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RegistroResponseDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Resultado = -1, Resultado_msj = stringData };
			}
		}

		public async Task<List<AutoComptesPendientesDto>> RPRObtenerComptesPendiente(string adm, string token)
		{
			ApiResponse<List<AutoComptesPendientesDto>> apiResponse;

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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AutoComptesPendientesDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
			CargarJsonGenRequest request = new() { json_str = json_str };
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos")	;
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

		public async Task<List<RPRxULDto>> RPRxUL(string rp, string token)
		{
			ApiResponse<List<RPRxULDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRxULOBTENER}?rp={rp}";
			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rp:{rp}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRxULDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RPRxULDetalleDto>> RPRxULDetalle(string ulId, string token)
		{
			ApiResponse<List<RPRxULDetalleDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RPRxULDETALLEOBTENER}?ulId={ulId}";
			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ulId:{ulId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRxULDetalleDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<JsonDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRItemVerCompteDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPRVerConteoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaDto> ValidarUL(string ul, string adm, string sm, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new HelperAPI();
			RprABRequest request = new() { UL = ul, AdmId = adm, Sm = sm };
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
					return new RespuestaDto() { resultado = -1, resultado_msj = "Hubo algun problema. Verifique el log local y de la api." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				try
				{
					var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					return new RespuestaDto() { resultado = -1, resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
				}
				catch
				{
					return new RespuestaDto() { resultado = -1, resultado_msj = stringData };
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RprResponseDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				try
				{
					var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					return new RprResponseDto() { Resultado = -1, Resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
				}
				catch
				{
					return new RprResponseDto() { Resultado = -1, Resultado_msj = stringData };
				}

			}
		}

		public async Task<RprResponseDto> ConfirmaBoxUl(string box, string ul, string adm, string sm, string token)
		{
			ApiResponse<RprResponseDto> apiResponse;

			HelperAPI helper = new HelperAPI();
			RprABRequest request = new() { Box = box, UL = ul, AdmId = adm, Sm = sm };
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RprResponseDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

				try
				{
					var res = JsonConvert.DeserializeObject<ExceptionValidation>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					return new RprResponseDto() { Resultado = -1, Resultado_msj = res.Detail ?? "Hubo algun problema. Verifique el log local y de la api." };
				}
				catch
				{
					return new RprResponseDto() { Resultado = -1, Resultado_msj = stringData };
				}
			}
		}

		public async Task<List<TRPendienteDto>> TRObtenerPendientes(string admId, string usuId, string titId, string token)
		{
			ApiResponse<List<TRPendienteDto>> apiResponse;

			HelperAPI helper = new();
			ObtenerTRPendientesRequest request = new() { admId = admId, titId = titId, usuId = usuId };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_PENDIENTES}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{admId} usu_id:{usuId} tit_id:{titId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRPendienteDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRAutSucursalesDto>> TRObtenerAutSucursales(string admId, string token)
		{
			ApiResponse<List<TRAutSucursalesDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_SUCURSALES}?admId={admId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{admId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRAutSucursalesDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRAutPIDto>> TRObtenerAutPI(string admId, string admIdLista, string token)
		{
			ApiResponse<List<TRAutPIDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_PI}?admId={admId}&admIdLista={admIdLista}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{admId} adm_id_lista: {admIdLista}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRAutPIDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRAutPIDetalleDto>> TRObtenerAutPIDetalle(string piCompte, string token)
		{
			ApiResponse<List<TRAutPIDetalleDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_PI_DETALLE}?piCompte={piCompte}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros pi_compte:{piCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRAutPIDetalleDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRAutDepoDto>> TRObtenerAutDepositos(string admId, string token)
		{
			ApiResponse<List<TRAutDepoDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_Depositos}?admId={admId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{admId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRAutDepoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRAutAnalizaDto>> TRAutAnaliza(string listaPi, string listaDepo, bool stkExistente, bool sustituto, int palletNro, string token)
		{
			ApiResponse<List<TRAutAnalizaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_Analiza}?listaPi={listaPi}&listaDepo={listaDepo}&stkExistente={stkExistente}&sustituto={sustituto}&palletNro={palletNro}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros listaPi:{listaPi} listaDepo:{listaDepo}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRAutAnalizaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<TRProductoParaAgregar>> TRObtenerSustituto(string pId, string listaDepo, string admIdDes, string tipo, string token)
		{
			ApiResponse<List<TRProductoParaAgregar>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_Sustituto}?pId={pId}&listaDepo={listaDepo}&admIdDes={admIdDes}&tipo={tipo}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros pId:{pId} listaDepo:{listaDepo}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRProductoParaAgregar>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> TRConfirmaAutorizaciones(string json, string admId, string usuId, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			TRConfirmaRequest request = new() { json = json, admId = admId, usuId = usuId };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_AUT_Confirma_Auto}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros json:{json}");
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

		public async Task<List<TRVerConteosDto>> TRVerConteos(string ti, string token)
		{
			ApiResponse<List<TRVerConteosDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_Ver_Conteos}?ti={ti}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ti:{ti}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TRVerConteosDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RespuestaDto>> TRValidarTransferencia(string ti, string admId, string usuId, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			TRValidarTransferenciaRequest request = new() { ti = ti, admId = admId, usuId = usuId };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_Validar_Transferencia}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ti:{ti}");
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

		public async Task<List<ProductoNCPIDto>> NCPICargarListaDeProductos(string tipo, string admId, string filtro, string id, string token)
		{
			ApiResponse<List<ProductoNCPIDto>> apiResponse;

			HelperAPI helper = new();
			NCPICargarListaDeProductosRequest request = new() { Tipo = tipo, AdmId = admId, Filtro = filtro, Id = id };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Productos}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros tipo:{tipo} admId:{admId} filtro:{filtro} id:{id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoNCPIDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<NCPICargaPedidoResponse>> NCPICargaPedido(NCPICargaPedidoRequest req, string token)
		{
			ApiResponse<List<NCPICargaPedidoResponse>> apiResponse;

			HelperAPI helper = new();
			NCPICargaPedidoRequest request = req;
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Cargar_Pedido}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros tipo:{req.tipo} admId:{req.admId} p_id:{req.pId} tipo_carga:{req.tipoCarga}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<NCPICargaPedidoResponse>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
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
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AutorizacionTIDto>>>(stringData) ?? new ApiResponse<List<AutorizacionTIDto>>(new List<AutorizacionTIDto>());
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

		public async Task<List<TipoMotivoDto>> ObtenerTiposMotivo(string token)
		{
			ApiResponse<List<TipoMotivoDto>> apiResponse;
			HelperAPI helper = new HelperAPI();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_TM}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoMotivoDto>>>(stringData) ?? new ApiResponse<List<TipoMotivoDto>>(new List<TipoMotivoDto>());
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

		public async Task<RespuestaGenerica<RespuestaDto>> VaidaProductoCarrito(TiProductoCarritoDto request, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_VALIDA_PROD_CARRITO}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros {JsonConvert.SerializeObject(request)}");
					return new() { Ok = false, Mensaje = "No se recepciono datos alguno." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK" };
				}
				else
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Algo no fue bien. Verifique el log." };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ResguardarProductoCarrito(TiProductoCarritoDto request, string token)
		{

			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_RESGUARDA_PROD_CARRITO}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros {JsonConvert.SerializeObject(request)}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK" };
				}
				else
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ControlSalidaTI(string ti, string adm, string usu, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_CONTROL_SALIDA}?ti={ti}&adm={adm}&usu={usu}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK" };
				}
				else
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj, Entidad = apiResponse.Data };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<TIRespuestaDto>> TIValidaPendiente(string usu, string token)
		{
			ApiResponse<TIRespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_VALIDA_PENDIENTE}?usu={usu}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<TIRespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<TIRespuestaDto> { Ok = true, Mensaje = "OK" };
				}
				else
				{
					return new RespuestaGenerica<TIRespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj, Entidad = apiResponse.Data };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> TIConfirma(TIRequestConfirmaDto confirma, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(confirma, token, out StringContent content);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_CONFIRMA}";

			response = await client.PostAsync(link, content);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK" };
				}
				else
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj, Entidad = apiResponse.Data };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
			}
		}

		public async Task<RespuestaGenerica<TIRespuestaDto>> TINueva_SinAu(string tipo, string adm, string usu, string token)
		{
			ApiResponse<TIRespuestaDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_NUEVA_SIN_AUTO}?tipo={tipo}&adm={adm}&usu={usu}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<TIRespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<TIRespuestaDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };
				}
				else
				{
					return new RespuestaGenerica<TIRespuestaDto> { Ok = false, Mensaje = apiResponse.Data.resultado_msj };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
			}
		}
		//
		public async Task<RespuestaGenerica<ProductoDepositoDto>> BuscarFechaVto(string pId, string bId, string token)
		{
			ApiResponse<ProductoDepositoDto> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TR_Busca_Vto}?pId={pId}&bId={bId}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoDepositoDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.P_Id.Equals(pId) && apiResponse.Data.Box_Id.Equals(bId))
				{
					return new RespuestaGenerica<ProductoDepositoDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };
				}
				else
				{
					return new RespuestaGenerica<ProductoDepositoDto> { Ok = true, Mensaje = "No se encontró el producto.", Entidad = new ProductoDepositoDto { P_Id = pId, Box_Id = bId, Ps_Fv = "" } };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
			}
		}

		public async Task<RespuestaGenerica<ProductoGenDto>> ObtenerProductosCargadosCtrlSalida(string tr, string user, string token)
		{
			ApiResponse<List<ProductoGenDto>> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_VER_CTRL_SALIDA}?tr={tr}&user={user}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{

					return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoGenDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

				return new RespuestaGenerica<ProductoGenDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
			}
		}

		//TI_CARGAR_CTRL_SALIDA
		public async Task<RespuestaGenerica<RespuestaDto>> EnviarProductosCtrl(List<ProductoGenDto> lista, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new HelperAPI();
			TRProdsCtrlSalDto prods = new TRProdsCtrlSalDto() { ProdsCargar = lista };
			HttpClient client = helper.InicializaCliente(prods, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_CARGAR_CTRL_SALIDA}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. ");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				if (apiResponse.Data.resultado == 0)
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = true, Entidad = apiResponse.Data };
				}
				else
				{
					return new RespuestaGenerica<RespuestaDto> { Ok = false, Entidad = apiResponse.Data, Mensaje = apiResponse.Data.resultado_msj };
				}
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new() { Ok = false, Mensaje = "Hubo un problema al intentar enviar los productos para el control de salida. Verifique log, de ser necesario." };
			}
		}

		public async Task<List<InfoProductoFamiliaDto>> ObtenerProductosPorFamilia(string ctaId, string fliaSelected, string token)
		{
			ApiResponse<List<InfoProductoFamiliaDto>> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{PRODUCTOS_POR_FAMILIA}?provList={ctaId}&pgList={fliaSelected}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProductoFamiliaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

				//return new RespuestaGenerica<ProductoGenDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
				return apiResponse.Data;
				/*
				 				apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBusquedaDto>>(stringData);
				return apiResponse.Data;
				 */
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new List<InfoProductoFamiliaDto>();
			}
		}

		public async Task<(List<ProductoNCPIDto>, MetadataGrid)> NCPICargarListaDeProductosPag(string tipo, string admId, string filtro, string id, string token, string Sort, string SortDir, int Registros, int Pagina)
		{
			ApiResponse<List<ProductoNCPIDto>> apiResponse;

			HelperAPI helper = new();
			NCPICargarListaDeProductosRequest request = new() { Tipo = tipo, AdmId = admId, Filtro = filtro, Id = id, Sort = Sort, Registros = Registros, SortDir = SortDir, Pagina = Pagina };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Productos_Pag}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros tipo:{tipo} admId:{admId} filtro:{filtro} id:{id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoNCPIDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return (apiResponse.Data ?? [], apiResponse.Meta??new());
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<(List<ProductoNCPIDto>, MetadataGrid)> NCPICargarListaDeProductosPag2(NCPICargarListaDeProductos2Request request, string token)
		{
			ApiResponse<List<ProductoNCPIDto>> apiResponse;

			HelperAPI helper = new();
			//NCPICargarListaDeProductos2Request request = new() { Tipo = tipo, AdmId = admId, Filtro = filtro, Id = id, Sort = Sort, Registros = Registros, SortDir = SortDir, Pagina = Pagina };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Productos_Pag2}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoNCPIDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return (apiResponse.Data ?? [], apiResponse.Meta??new());
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<OrdenDeCompraListDto> CargarOrdenesDeCompraList(string ctaId, string admId, string usuId, string token)
		{
			ApiResponse<List<OrdenDeCompraListDto>> apiResponse;

			try
			{
				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Cargar_Lista}?ctaId={ctaId}&admId={admId}&usuId={usuId}";

				//response = await client.GetAsync(link);
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraListDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de las cuentas de familia de proveedores. Verifique.");
					}
					return apiResponse.Data;
				}
				else
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					return new List<OrdenDeCompraListDto>();
				}
			}
			catch (NegocioException )
			{
				throw;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error al intentar obtener las Familia de Proveedores.");
				throw;
			}
			
		}

		public async Task<List<ProductoParaOcDto>> CargarProductosDeOC(CargarProductoParaOcRequest req, string token)
		{
			ApiResponse<List<ProductoParaOcDto>> apiResponse;

			HelperAPI helper = new();
			CargarProductoParaOcRequest request = req;
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Cargar_Detalle}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros Oc_Compte:{req.Oc_Compte} admId:{req.Adm_Id} usu_id:{req.Usu_Id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoParaOcDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<OrdenDeCompraTopeDto>> CargarTopesDeOC(string admId, string token)
		{
			ApiResponse<List<OrdenDeCompraTopeDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Tope}?admId={admId}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros admId:{admId}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraTopeDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos")		;
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<OrdenDeCompraConceptoDto>> CargarResumenDeOC(CargarResumenDeOCRequest req, string token)
		{
			ApiResponse<List<OrdenDeCompraConceptoDto>> apiResponse;

			HelperAPI helper = new();
			CargarResumenDeOCRequest request = req;
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Resumen}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros Oc_Compte:{req.Oc_Compte} admId:{req.Adm_Id} usu_id:{req.Usu_Id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraConceptoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarOrdenDeCompra(ConfirmarOCRequest request, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Confirmar}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data };
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<OrdenDeCompraDto>> ObtenerOrdenDeCompraPorOcCompte(string ocCompte, string token)
		{
			ApiResponse<List<OrdenDeCompraDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_ObtenerPorOcCompte}?ocCompte={ocCompte}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ocCompte:{ocCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<(List<OrdenDeCompraConsultaDto>, MetadataGrid)> CargarOrdenDeCompraConsultaLista(BuscarOrdenesDeCompraRequest request, string token)
		{
			ApiResponse<List<OrdenDeCompraConsultaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Lista}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraConsultaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<OrdenDeCompraDetalleDto>> CargarDetalleDeOC(string oc_compte, string token)
		{
			ApiResponse<List<OrdenDeCompraDetalleDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Detalle}?oc_compte={oc_compte}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros oc_compte:{oc_compte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraDetalleDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<OrdenDeCompraRprAsociadasDto>> CargarRprAsociadaDeOC(string oc_compte, string token)
		{
			ApiResponse<List<OrdenDeCompraRprAsociadasDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Rpr_Asociada}?oc_compte={oc_compte}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros oc_compte:{oc_compte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeCompraRprAsociadasDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ModificarOrdenDeCompra(ModificarOCRequest request, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{OC_Modificar}";

			response = await client.PutAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data };
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}
	}
}
