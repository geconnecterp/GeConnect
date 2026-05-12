using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    [Authorize]
    public class ProductoFactController : ControladorBaseCaja
    {
        private readonly ICajaServicio _cajaServicio;
        private readonly IProductoFactServicio _productoFactServicio;
        private readonly IReportesConfigService _reportesConfigService;
        private readonly IReportesService _reportesService;

        public ProductoFactController(
            IOptions<AppSettings> options,
            ICajaServicio cajaServicio,
            IProductoFactServicio productoFactServicio, // ✅ INYECTAR
            IHttpContextAccessor httpContext,
            IReportesConfigService reportesConfigService,
            IReportesService reportesService,
            ILogger<ProductoFactController> logger) : base(options, httpContext, logger)
        {
            _cajaServicio = cajaServicio;
            _productoFactServicio = productoFactServicio; // ✅ ASIGNAR
            _reportesConfigService = reportesConfigService;
            _reportesService = reportesService;
        }

        public IActionResult Index()
        {
            return View();
        }


        // tengo que generar una action via post que se llame ObtenerProductosDatosPrefactura
        // que me permita recepcionar los distintos cpf_nro en una lista y luego ir iterandola
        // para obtener todos los productos desde la logica de ObtenerProductoDatos. 
        // Por un lado se debera encapsular el codigo de la action para poder ser invocada desde 
        // multiples action y por el otro lado generar una variable de session para ir acumuladno
        // los productos obtenidos por cada prefactura. 

        private async Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatosCommon(
            string tipoValor,
            string valor,
            string listaPreciosId,
            string canalId,
            string identificadorCliente,
            decimal cantidad = 1,
            bool bulto = true

            )
        {
            var cajaActual = CajaActual;
            var clienteActual = ClienteActual;

            // ❼ CONSTRUIR REQUEST DTO
            var request = new ProductoDatosRequestDto
            {
                tipo_valor = tipoValor,
                valor = valor.Trim(),
                lp_id = listaPreciosId,
                adm_id = cajaActual.AdmId ?? AdministracionId,
                cantidad = cantidad,
                bulto = bulto,
                ctc_id = canalId,
                cta_id = identificadorCliente,
                ctac_dto = clienteActual?.ctac_dto_operacion ?? 0
            };

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO:");
            _logger?.LogInformation($"   - tipo_valor: {request.tipo_valor}");
            _logger?.LogInformation($"   - valor: {request.valor}");
            _logger?.LogInformation($"   - lp_id: {request.lp_id}");
            _logger?.LogInformation($"   - adm_id: {request.adm_id}");
            _logger?.LogInformation($"   - cantidad: {request.cantidad}");
            _logger?.LogInformation($"   - bulto: {request.bulto}");
            _logger?.LogInformation($"   - ctc_id: {request.ctc_id}");
            _logger?.LogInformation($"   - cta_id: {request.cta_id}");
            _logger?.LogInformation($"   - ctac_dto: {request.ctac_dto}");
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            // ❽ INVOCAR SERVICIO
            var token = TokenCookie;
            if (string.IsNullOrEmpty(token))
            {
                _logger?.LogError("❌ No se pudo obtener el token de autenticación");
                throw new NegocioException("Sesión expirada. Por favor, vuelva a iniciar sesión.");
            }

            _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.ObtenerProductoDatos...");
            return await _productoFactServicio.ObtenerProductoDatos(request, token);
        }

        /// <summary>
        /// ✅ NUEVO: Obtiene los datos de un producto para agregarlo a la factura
        /// </summary>
        /// <param name="tipoValor">Tipo de búsqueda: P (Producto), F (Pre-Factura), C (Cotización)</param>
        /// <param name="valor">Valor según tipo: ID producto/barras, ID prefactura, ID cotización</param>
        /// <param name="cantidad">Cantidad del producto (default: 1)</param>
        /// <param name="bulto">Si la cantidad es por bulto (default: true)</param>
        /// <returns>JSON con datos del producto o error</returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerProductoDatos(
            string tipoValor,
            string valor,
            decimal cantidad = 1,
            bool bulto = true)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 OBTENER DATOS DE PRODUCTO - INICIO");
                _logger?.LogInformation($"   Parámetros recibidos:");
                _logger?.LogInformation($"   - Tipo Valor: {tipoValor}");
                _logger?.LogInformation($"   - Valor: {valor}");
                _logger?.LogInformation($"   - Cantidad: {cantidad}");
                _logger?.LogInformation($"   - Bulto: {bulto}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ VALIDAR PARÁMETROS DE ENTRADA
                if (string.IsNullOrWhiteSpace(tipoValor))
                {
                    _logger?.LogWarning("❌ Parámetro 'tipoValor' es requerido");
                    return Json(new { ok = false, mensaje = "Debe especificar el tipo de búsqueda" });
                }

                if (string.IsNullOrWhiteSpace(valor))
                {
                    _logger?.LogWarning("❌ Parámetro 'valor' es requerido");
                    return Json(new { ok = false, mensaje = "Debe ingresar un código de producto" });
                }

                // Validar tipo de búsqueda
                tipoValor = tipoValor.ToUpper().Trim();
                if (tipoValor != "P" && tipoValor != "F" && tipoValor != "C")
                {
                    _logger?.LogWarning($"❌ Tipo de valor inválido: {tipoValor}");
                    return Json(new { ok = false, mensaje = "Tipo de búsqueda inválido. Use P (Producto), F (Pre-Factura) o C (Cotización)" });
                }

                // Validar cantidad
                if (cantidad <= 0)
                {
                    _logger?.LogWarning($"❌ Cantidad inválida: {cantidad}");
                    return Json(new { ok = false, mensaje = "La cantidad debe ser mayor a cero" });
                }

                // ❷ VALIDAR CLIENTE ACTUAL (REQUERIDO)
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado. Por favor, identifique un cliente primero." });
                }

                _logger?.LogInformation("✅ Cliente actual obtenido de sesión:");
                _logger?.LogInformation($"   - Nombre: {clienteActual.cta_denominacion}");
                _logger?.LogInformation($"   - Origen: {clienteActual.Origen} ({clienteActual.valida_desc})");
                _logger?.LogInformation($"   - CTA_ID: {clienteActual.cta_id}");
                _logger?.LogInformation($"   - Documento: {clienteActual.cta_documento}");
                _logger?.LogInformation($"   - CTC_ID: {clienteActual.ctc_id}");
                _logger?.LogInformation($"   - Descuento Operación: {clienteActual.ctac_dto_operacion}");

                // ❸ VALIDAR CAJA ACTUAL (REQUERIDO)
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogWarning("❌ No hay caja seleccionada en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta. Por favor, inicie sesión de caja." });
                }

                _logger?.LogInformation("✅ Caja actual obtenida de sesión:");
                _logger?.LogInformation($"   - Caja ID: {cajaActual.CajaId}");
                _logger?.LogInformation($"   - Administración: {cajaActual.AdmId}");
                _logger?.LogInformation($"   - Lista Precios (Min): {cajaActual.Caja.lp_id_min}");
                _logger?.LogInformation($"   - Lista Precios (Max): {cajaActual.Caja.lp_id_may}");

                // ❹ DETERMINAR LISTA DE PRECIOS
                string listaPreciosId;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    listaPreciosId = cajaActual.Caja.lp_id_min ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente Consumidor Final → Lista de Precios MIN: {listaPreciosId}");
                }
                else if (origenUpper == "C") // Cliente Registrado
                {
                    listaPreciosId = cajaActual.Caja.lp_id_may ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente Registrado → Lista de Precios MAX: {listaPreciosId}");
                }
                else
                {
                    // Fallback: usar mínima
                    listaPreciosId = cajaActual.Caja.lp_id_min ?? string.Empty;
                    _logger?.LogWarning($"⚠️ Origen desconocido '{origenUpper}' → Usando Lista de Precios MIN: {listaPreciosId}");
                }

                if (string.IsNullOrEmpty(listaPreciosId))
                {
                    _logger?.LogError("❌ No se pudo determinar la lista de precios");
                    return Json(new { ok = false, mensaje = "Error de configuración: No hay lista de precios asignada a la caja" });
                }

                // ❺ DETERMINAR CANAL (ctc_id)
                // Usar el canal del cliente si existe, sino determinar por origen
                string canalId = clienteActual.ctc_id ?? string.Empty;

                if (string.IsNullOrEmpty(canalId))
                {
                    if (origenUpper == "F")
                    {
                        canalId = "MI"; // Minorista (Consumidor Final)
                        _logger?.LogInformation($"✅ Canal determinado por origen (CF): {canalId}");
                    }
                    else
                    {
                        canalId = "MA"; // Mayorista (Cliente Registrado)
                        _logger?.LogInformation($"✅ Canal determinado por origen (Registrado): {canalId}");
                    }
                }
                else
                {
                    _logger?.LogInformation($"✅ Canal obtenido del cliente: {canalId}");
                }

                // ❻ DETERMINAR IDENTIFICADOR DEL CLIENTE (cta_id)
                // Según origen: F (documento) o C (cta_id)
                string identificadorCliente;

                if (origenUpper == "F")
                {
                    identificadorCliente = clienteActual.cta_documento ?? string.Empty;
                    _logger?.LogInformation($"✅ Identificador para CF (documento): {identificadorCliente}");
                }
                else
                {
                    identificadorCliente = clienteActual.cta_id ?? string.Empty;
                    _logger?.LogInformation($"✅ Identificador para Registrado (cta_id): {identificadorCliente}");
                }

                if (string.IsNullOrEmpty(identificadorCliente))
                {
                    _logger?.LogError("❌ No se pudo determinar el identificador del cliente");
                    return Json(new { ok = false, mensaje = "Error: Datos incompletos del cliente" });
                }

                // ❼ CONSTRUIR Y ENVIAR REQUEST AL SERVICIO - se encapsula en el método ObtenerProductoDatosCommon para poder ser reutilizado desde otras acciones (como la de prefactura)
                var resultado = await ObtenerProductoDatosCommon(tipoValor, valor, listaPreciosId, canalId, identificadorCliente, cantidad, bulto);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // 8 PROCESAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al obtener datos del producto" });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Servicio retornó error: {resultado.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado.Mensaje ?? "Error al obtener datos del producto"
                    });
                }

                // ❿ VALIDAR ENTIDAD DE RESPUESTA
                if (resultado.ListaEntidad == null)
                {
                    _logger?.LogError("❌ La entidad de respuesta es null");
                    return Json(new { ok = false, mensaje = "No se recibieron datos del producto" });
                }

                // ⓫ VALIDA SI EL PRODUCTO FUE SOLICITADO CON EL CODIGO DE BARRAS O CON EL ID DE PRODUCTO
                //siempre sera un solo producto
                var prod = resultado.ListaEntidad[0];
                //tiene barrado y no lo utilizó
                if (prod.sin_scan_con_barrado)
                {
                    _logger?.LogError("❌ Tiene barrado y no lo utilizó");
                    return Json(new { ok = false, mensaje = "El producto tiene código de barras y debe ser escaneado o ingresado manualmente." });
                }

                var productos = resultado.ListaEntidad;

                // ⓫ DETECTAR WARNINGS O ERRORES EN LA RESPUESTA
                if (resultado.EsWarn)
                {
                    _logger?.LogWarning($"⚠️ WARNING del servidor: {resultado.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        esWarning = true,
                        mensaje = resultado.Mensaje,
                        producto = productos
                    });
                }

                if (resultado.EsError)
                {
                    _logger?.LogError($"❌ ERROR del servidor: {resultado.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        esError = true,
                        mensaje = resultado.Mensaje
                    });
                }

                //// ⓬ ÉXITO
                //_logger?.LogInformation("═══════════════════════════════════════════════════");
                //_logger?.LogInformation("✅ PRODUCTO OBTENIDO EXITOSAMENTE");
                //_logger?.LogInformation($"   Código: {productos.respuesta}");
                //_logger?.LogInformation($"   Mensaje: {productos.respuesta_msj}");
                //_logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new
                {
                    ok = true,
                    mensaje = resultado.Mensaje ?? "Producto cargado correctamente",
                    producto = productos
                });
            }
            catch (NegocioException ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en ObtenerProductoDatos: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo transcurrido antes del error: {stopwatch.ElapsedMilliseconds}ms");
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en ObtenerProductoDatos: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo transcurrido antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al procesar el producto. Por favor, intente nuevamente."
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> BusquedaBase(string busqueda, bool validarEstado = false, bool acumularProductos = false, string modulo = "RPR")
        {
            try
            {
                ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
                if (string.IsNullOrEmpty(busqueda))
                {
                    return Json(new { error = false, producto });
                }

                //InicializaVariablesBusquedaBase();

                if (busqueda.Trim().Length < 6)
                {
                    busqueda = busqueda.Trim().PadLeft(6, '0');
                }
                BusquedaBase buscar = new BusquedaBase
                {
                    Administracion = AdministracionId,
                    Busqueda = busqueda,
                    //DescuentoCli = _busqueda.DescuentoCli,
                    //ListaPrecio = _busqueda.ListaPrecio,
                    //TipoOperacion = _busqueda.TipoOperacion
                };

                producto = await _productoFactServicio.BusquedaBaseProductos(buscar, TokenCookie);

                if (producto != null && !string.IsNullOrEmpty(producto.P_id))
                {
                    bool warn = false;
                    string msg = string.Empty;
                    //validación de Estado
                    if (!producto.P_activo.Equals("S") && validarEstado)
                    {
                        //se valida que no esta activo. Valores Noactivo Discontinuo
                        return Json(new { error = true, msg = $"El producto {producto.P_desc} se encuentra {producto.Msj}" });
                    }
                    ////Validación si pertenece o no al proveedor
                    //if (!modulo.Trim().ToUpper().Equals("INV"))
                    //{
                    //    if (modulo.ToUpper().Equals("RTI"))
                    //    {
                    //        //verificamos si el producto se encuentra en el remito.
                    //        var resp = await _remitoSv.VerificaProductoEnRemito(rm: RemitoActual.re_compte, pId: producto.P_id, TokenCookie);
                    //        if (resp.resultado != 0)
                    //        {
                    //            return Json(new { error = true, msg = resp.resultado_msj });
                    //        }
                    //    }
                    //    else
                    //    {
                    //        if (AutorizacionPendienteSeleccionada != null &&
                    //            !AutorizacionPendienteSeleccionada.Cta_id.Equals(producto.Cta_id) && validarEstado)
                    //        {
                    //            warn = true;
                    //            msg = $"El Producto NO pertenece al actual proveedor. Pertenece al Proveedor {producto.Cta_denominacion}.";
                    //        }
                    //    }
                    //}

                    //se resguarda el producto recien buscado.
                    ProductoBase = producto;
                    if (acumularProductos)
                    {
                        var productos = ProductosSeleccionados;
                        productos.Add(producto);
                        ProductosSeleccionados = productos;
                    }
                    return Json(new { error = false, producto, warn, msg, });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = "El producto no ha sido identificado.", producto = new ProductoBusquedaDto() { P_id = "NO" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hubo un error en la busqueda avanzada");
                return Json(new { error = true, msg = "Algo no salió bien. Vuelva a intentarlo." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BusquedaAvanzada(string ri01, string ri02, bool act, bool dis, bool ina, bool cstk, bool sstk, string buscar, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1)
        {
            return await BusquedaAvanzada(ri01, ri02, act, dis, ina, cstk, sstk, buscar, buscaNew, _productoFactServicio, sort, sortDir, pag);
        }

        /// <summary>
        /// Búsqueda avanzada V02 que devuelve JSON con ProductoListaDto para ofertas
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BusquedaAvanzadaV02(
            string ri01,        // ✅ Proveedor (por defecto "")
            string ri02,        // ✅ Rubro (por defecto "")
            string ri03,        // ✅ Familia (por defecto "%")
            bool act,           // ✅ Buscar activos
            bool dis,           // ✅ Buscar discontinuos
            bool ina,           // ✅ Buscar inactivos
            bool cstk,          // ✅ Con stock
            bool sstk,          // ✅ Sin stock
            string buscar,      // ✅ Texto de búsqueda
            string lp_id,       // ✅ CRÍTICO: Lista de precios
            bool buscaNew = true,
            string sort = "p_desc",
            string sortDir = "asc",
            int pag = 1)
        {
            try
            {
                // ✅ Validar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) ||
                    (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
                {
                    return new JsonResult(new
                    {
                        error = true,
                        msg = "Sesión expirada. Debe autenticarse nuevamente.",
                        productos = new List<ProductoListaDto>(),
                        redirect = true,
                        redirectUrl = Url.Action("Login", "Token", new { area = "Seguridad" })
                    });
                }

                // ✅ CRÍTICO: Usa lp_id del parámetro si viene, sino de sesión
                lp_id = string.IsNullOrEmpty(lp_id) ? LP_Id : lp_id;

                // ✅ Delegación al método base optimizado
                return await BusquedaAvanzadaV02(
                    ri01,
                    ri02,
                    ri03 ?? "%",  // ✅ Usa "%" si viene null
                    act,
                    dis,
                    ina,
                    cstk,
                    sstk,
                    buscar,
                    lp_id,        // ✅ Parámetro recibido correctamente
                    buscaNew,
                    _productoFactServicio,
                    sort,
                    sortDir,
                    pag
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en búsqueda avanzada V02");
                return new JsonResult(new
                {
                    error = true,
                    msg = "Error interno en la búsqueda de productos",
                    productos = new List<ProductoListaDto>(),
                    metadata = new { totalCount = 0, totalPages = 0, pageSize = 0, currentPage = pag }
                });
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // NUEVA ACTION: CALCULAR FILAS Y TOTALES
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// ✅ NUEVO: Calcula totales de factura invocando SPGECO_CAJA_Ope_Calcula_Filas
        /// </summary>
        /// <param name="request">Request con datos del cliente, totales y JSON de productos</param>
        /// <returns>JSON con subtotales, sorteos y datos impositivos</returns>
        [HttpPost]
        public async Task<JsonResult> CalcularFilas([FromBody] CalcularFilasReqDto request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔢 CALCULAR FILAS - INICIO");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❷ VALIDAR REQUEST
                if (request == null)
                {
                    _logger?.LogWarning("❌ Request es null");
                    return Json(new { ok = false, mensaje = "Datos inválidos" });
                }

                _logger?.LogInformation($"   Request recibido:");
                _logger?.LogInformation($"   - caja_id: {request.caja_id}");
                _logger?.LogInformation($"   - usu_id: {request.usu_id}");
                _logger?.LogInformation($"   - cta_id: {request.cta_id}");
                _logger?.LogInformation($"   - tot_rows: {request.tot_rows}");
                _logger?.LogInformation($"   - tot_cantidad: {request.tot_cantidad}");
                _logger?.LogInformation($"   - tot_pvta: {request.tot_pvta}");

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation($"REQUEST: {JsonConvert.SerializeObject(request)}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                // ❸ VALIDAR QUE HAYA PRODUCTOS
                if (string.IsNullOrEmpty(request.json_p) || request.json_p == "[]")
                {
                    _logger?.LogWarning("❌ No hay productos en el JSON");
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un producto" });
                }

                // ❹ VALIDAR DATOS DE CAJA
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogError("❌ No hay caja en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // ❺ COMPLETAR DATOS FALTANTES DEL REQUEST
                if (string.IsNullOrEmpty(request.caja_id))
                    request.caja_id = cajaActual.CajaId ?? string.Empty;

                if (string.IsNullOrEmpty(request.usu_id))
                    request.usu_id = UserName ?? string.Empty;

                if (string.IsNullOrEmpty(request.adm_id))
                    request.adm_id = cajaActual.AdmId ?? AdministracionId;

                if (request.caja_nro_cierre.ToInt() == 0)
                    request.caja_nro_cierre = cajaActual.Caja.caja_nro_cierre ?? string.Empty;

                if (string.IsNullOrEmpty(request.caja_nro_proceso))
                    request.caja_nro_proceso = cajaActual.Caja.caja_nro_proceso ?? string.Empty;

                // ❻ VALIDAR DATOS DE CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogError("❌ No hay cliente en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado" });
                }

                // ❼ COMPLETAR DATOS DEL CLIENTE
                if (string.IsNullOrEmpty(request.cta_id))
                {
                    // Según origen: F (documento) o C (cta_id)
                    string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";
                    request.cta_id = origenUpper == "F"
                        ? (clienteActual.cta_documento ?? string.Empty)
                        : (clienteActual.cta_id ?? string.Empty);
                }

                if (request.ctac_dto == 0)
                    request.ctac_dto = clienteActual.ctac_dto_operacion;

                if (string.IsNullOrEmpty(request.ctc_id))
                    request.ctc_id = clienteActual.ctc_id ?? string.Empty;

                if (string.IsNullOrEmpty(request.afip_id))
                    request.afip_id = clienteActual.afip_id ?? string.Empty;

                if (string.IsNullOrEmpty(request.afip_desc))
                    request.afip_desc = clienteActual.afip_desc ?? string.Empty;

                if (string.IsNullOrEmpty(request.cta_ib_nro))
                    request.cta_ib_nro = clienteActual.cta_ib_nro ?? string.Empty;

                if (string.IsNullOrEmpty(request.ib_id))
                    request.ib_id = clienteActual.ib_id ?? string.Empty;

                // ❽ COMPLETAR DATOS DE COMPROBANTE (si existen en sesión)
                // TODO: Completar según datos de factura en sesión
                // request.tco_letra = ...
                // request.tco_id = ...
                if (string.IsNullOrEmpty(request.tco_letra))
                    request.tco_letra = clienteActual.tco_letra ?? string.Empty;

                request.tco_id = string.Empty; // No se obtiene de cliente, se asigna vacío para que el SP lo determine
                request.tco_id_ori = string.Empty;
                request.cm_compte_ori = string.Empty;

                if (string.IsNullOrEmpty(request.pib_cert))
                    request.pib_cert = clienteActual.pib_cert ?? string.Empty;

                if (request.pib_cert_vto == DateTime.MinValue)
                    request.pib_cert_vto = clienteActual.pib_cert_vto;

                if (string.IsNullOrEmpty(request.piva_cert))
                    request.piva_cert = clienteActual.piva_cert ?? string.Empty;

                if (request.piva_cert_vto == DateTime.MinValue)
                    request.piva_cert_vto = clienteActual.piva_cert_vto;



                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📋 REQUEST COMPLETO CALCULO:");
                _logger?.LogInformation($"{JsonConvert.SerializeObject(request)}");              
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❾ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada" });
                }

                _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.CalcularFilas...");
                var resultado = await _productoFactServicio.CalcularFilas(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📋 RESPONSE COMPLETO:");
                _logger?.LogInformation($"{JsonConvert.SerializeObject(resultado)}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                // ❿ VALIDAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al calcular totales" });
                }

                // ⓫ GUARDAR JSON DE PRODUCTOS IMPOSITIVOS EN SESIÓN, SUBTOTALES Y SORTEOS
                if (!string.IsNullOrEmpty(resultado.json_p))
                {
                    FacturaProductos = [];
                    var prods = JsonConvert.DeserializeObject<List<ProductoFactJsonDto>>(resultado.json_p);
                    FacturaProductos = prods ?? [];

                    //HttpContext.Session.SetString("FacturaProductosImpositivos", resultado.json_p);
                    _logger?.LogInformation("✅ JSON de productos impositivos guardado en sesión");

                    FacturaSubtotales = [];
                    if (!string.IsNullOrEmpty(resultado.json_subtotal))
                    {
                        var subtots = JsonConvert.DeserializeObject<List<FactSubtotalJsonDto>>(resultado.json_subtotal);
                        FacturaSubtotales = subtots ?? [];
                        _logger?.LogInformation("✅ JSON de subtotales guardado en sesión");
                    }

                    FacturaSorteos = [];
                }

                // ⓬ RETORNAR RESPUESTA
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ CÁLCULO COMPLETADO EXITOSAMENTE");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new
                {
                    ok = true,
                    mensaje = "Totales calculados correctamente",
                    json_subtotal = resultado.json_subtotal,
                    json_sorteo = resultado.json_sorteo,
                    json_p = resultado.json_p
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en CalcularFilas: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al calcular totales. Por favor, intente nuevamente."
                });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Obtiene las listas de precios disponibles
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerListasPrecios()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // TODO: Implementar servicio que obtenga las listas de precios
                // var resultado = await _productoFactServicio.ObtenerListasPrecios(TokenCookie);

                // MOCK para desarrollo:
                var listas = new[]
                {
            new { lp_id = "001", lp_desc = "Mayorista" },
            new { lp_id = "002", lp_desc = "Minorista" },
            new { lp_id = "003", lp_desc = "Distribuidora" }
        };

                return Json(new
                {
                    ok = true,
                    listas = listas,
                    lp_actual = LP_Id // Lista actual del usuario
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener listas de precios");
                return Json(new { ok = false, mensaje = "Error al obtener listas de precios" });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Cambia la lista de precios activa
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CambiarListaPrecios(string lp_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                if (string.IsNullOrEmpty(lp_id))
                {
                    return Json(new { ok = false, mensaje = "Debe especificar una lista de precios" });
                }

                // TODO: Implementar servicio que cambie la lista de precios en sesión
                // var resultado = await _productoFactServicio.CambiarListaPrecios(lp_id, TokenCookie);

                // MOCK para desarrollo:LP_Id
                LP_Id = lp_id; // Actualizar variable de sesión

                return Json(new
                {
                    ok = true,
                    mensaje = "Lista de precios cambiada exitosamente",
                    lp_id = lp_id
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cambiar lista de precios");
                return Json(new { ok = false, mensaje = "Error al cambiar lista de precios" });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Busca pre-facturas según filtros
        /// Implementa el requerimiento del 23 de abril de 2026
        /// 
        /// INVOCA: SPGECO_CAJA_B_Prefacturas con parámetros:
        /// - @sec_id: 'CAJA' (fijo)
        /// - @cta_id: ID del cliente registrado o '%' para CF
        /// - @documento: Documento del CF o '%' para cliente registrado
        /// - @usada: 'N' (solo pendientes) o '%' (todas)
        /// </summary>
        /// <param name="solo_pendientes">Si es true, filtra solo pendientes (usada='N')</param>
        [HttpPost]
        public async Task<JsonResult> ObtenerPreFacturas(bool solo_pendientes = true)
        {
            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 OBTENER PRE-FACTURAS - CONTROLLER v2.0");
                _logger?.LogInformation($"   solo_pendientes: {solo_pendientes}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // 🛑 VALIDAR CLIENTE ACTUAL (OBLIGATORIO)
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado en sesión");
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe seleccionar un cliente antes de buscar pre-facturas",
                        prefacturas = new List<PrefacturaResDto>()
                    });
                }

                _logger?.LogInformation("✅ Cliente actual obtenido de sesión:");
                _logger?.LogInformation($"   - Nombre: {clienteActual.cta_denominacion}");
                _logger?.LogInformation($"   - Origen: {clienteActual.Origen} ({clienteActual.valida_desc})");
                _logger?.LogInformation($"   - CTA_ID: {clienteActual.cta_id}");
                _logger?.LogInformation($"   - Documento: {clienteActual.cta_documento}");

                // ③ CONSTRUIR REQUEST DTO SEGÚN TIPO DE CLIENTE
                var request = new PrefacturaReqDto
                {
                    sec_id = "CAJA", // ✅ FIJO según requerimiento
                    usada = solo_pendientes ? "N" : "%" // ✅ Checkbox: N (pendientes) o % (todos)
                };

                // ✅ LÓGICA DIFERENCIAL: Cliente registrado vs Consumidor Final
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "C") // Cliente Registrado
                {
                    request.cta_id = clienteActual.cta_id ?? "%";
                    request.documento = "%"; // ✅ Anular documento
                    _logger?.LogInformation($"✅ Cliente REGISTRADO → cta_id: {request.cta_id}, documento: %");
                }
                else // Consumidor Final (F) o cualquier otro
                {
                    request.cta_id = "%"; // ✅ Anular cta_id
                    request.documento = clienteActual.cta_documento ?? "%";
                    _logger?.LogInformation($"✅ Cliente CONSUMIDOR FINAL → cta_id: %, documento: {request.documento}");
                }

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO:");
                _logger?.LogInformation($"   - sec_id: {request.sec_id}");
                _logger?.LogInformation($"   - cta_id: {request.cta_id}");
                _logger?.LogInformation($"   - documento: {request.documento}");
                _logger?.LogInformation($"   - usada: {request.usada} ('{(solo_pendientes ? "Solo pendientes" : "Todas")}')");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❹ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada", prefacturas = new List<PrefacturaResDto>() });
                }

                _logger?.LogInformation("📡 Invocando servicio IProductoFactServicio.ObtenerPrefactura...");
                var resultado = await _productoFactServicio.ObtenerPrefactura(request, token);

                // ❺ VALIDAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Error al buscar pre-facturas",
                        prefacturas = new List<PrefacturaResDto>()
                    });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error del servicio: {resultado.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado.Mensaje ?? "Error al buscar pre-facturas",
                        prefacturas = new List<PrefacturaResDto>()
                    });
                }

                // ❻ RESPUESTA EXITOSA
                var prefacturas = resultado.ListaEntidad ?? new List<PrefacturaResDto>();

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation($"✅ SE ENCONTRARON {prefacturas.Count} PRE-FACTURAS");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(new
                {
                    ok = true,
                    mensaje = prefacturas.Count > 0
                        ? $"Se encontraron {prefacturas.Count} pre-factura(s)"
                        : "No hay pre-facturas disponibles",
                    prefacturas = prefacturas
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ EXCEPCIÓN en ObtenerPreFacturas");
                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al buscar pre-facturas. Por favor, intente nuevamente.",
                    prefacturas = new List<PrefacturaResDto>()
                });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Busca cotizaciones para un cliente
        /// Implementa el requerimiento del 23 de abril
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerCotizaciones(string cta_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 BUSCAR COTIZACIONES - CONTROLLER");
                _logger?.LogInformation($"   cta_id: {cta_id}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ VALIDAR CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado");
                    return Json(new { ok = false, mensaje = "Debe seleccionar un cliente primero" });
                }

                // ❷ CONSTRUIR REQUEST
                // ✅ SOLO RECIBE CTA_ID (no maneja consumidor final según SP)
                var request = new CotizacionReqDto
                {
                    cta_id = clienteActual.cta_id ?? cta_id ?? "%"
                };

                _logger?.LogInformation($"✅ Buscando cotizaciones para cta_id: {request.cta_id}");

                // ❸ INVOCAR SERVICIO
                var resultado = await _productoFactServicio.ObtenerCotizacion(request, TokenCookie);

                if (resultado == null || !resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error al buscar: {resultado?.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado?.Mensaje ?? "Error al buscar cotizaciones",
                        cotizaciones = new List<CotizacionResDto>()
                    });
                }

                _logger?.LogInformation($"✅ Se encontraron {resultado.ListaEntidad?.Count ?? 0} cotizaciones");

                return Json(new
                {
                    ok = true,
                    mensaje = "OK",
                    cotizaciones = resultado?.ListaEntidad?
                                        .OrderByDescending(x => x.pre_fecha)
                                        .ThenByDescending(x => x.pree_id)
                                        .ToList() ?? new List<CotizacionResDto>()
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar cotizaciones");
                return Json(new { ok = false, mensaje = "Error al buscar cotizaciones", cotizaciones = new List<CotizacionResDto>() });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Obtiene productos de múltiples pre-facturas y acumula en sesión
        /// CORREGIDO: Mapeo COMPLETO de todos los campos del DTO ProductoFactJsonDto
        /// </summary>
        /// <param name="cpf_nros">Lista de códigos de pre-factura (cpf_nro)</param>
        /// <returns>JSON con productos acumulados o error</returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDatosPrefactura([FromBody] List<string> cpf_nros)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 OBTENER PRODUCTOS DE PRE-FACTURAS - INICIO v2.0");
                _logger?.LogInformation($"   Cantidad de pre-facturas: {cpf_nros?.Count ?? 0}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❷ VALIDAR PARÁMETROS
                if (cpf_nros == null || cpf_nros.Count == 0)
                {
                    _logger?.LogWarning("❌ No se recibieron códigos de pre-factura");
                    return Json(new { ok = false, mensaje = "Debe seleccionar al menos una pre-factura" });
                }

                // ❸ VALIDAR CLIENTE Y CAJA (REQUERIDOS)
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado" });
                }

                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogWarning("❌ No hay caja abierta");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // ❹ DETERMINAR LISTA DE PRECIOS, CANAL E IDENTIFICADOR
                string listaPreciosId;
                string canalId;
                string identificadorCliente;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    listaPreciosId = cajaActual.Caja.lp_id_min ?? string.Empty;
                    canalId = clienteActual.ctc_id ?? "MI";
                    identificadorCliente = clienteActual.cta_documento ?? string.Empty;

                    _logger?.LogInformation($"✅ Cliente CF → LP: {listaPreciosId}, Canal: {canalId}, Doc: {identificadorCliente}");
                }
                else // Cliente Registrado
                {
                    listaPreciosId = cajaActual.Caja.lp_id_may ?? string.Empty;
                    canalId = clienteActual.ctc_id ?? "MA";
                    identificadorCliente = clienteActual.cta_id ?? string.Empty;

                    _logger?.LogInformation($"✅ Cliente Registrado → LP: {listaPreciosId}, Canal: {canalId}, CTA_ID: {identificadorCliente}");
                }

                if (string.IsNullOrEmpty(identificadorCliente))
                {
                    _logger?.LogError("❌ No se pudo determinar el identificador del cliente");
                    return Json(new { ok = false, mensaje = "Error: Datos incompletos del cliente" });
                }

                // ❺ INICIALIZAR LISTA DE ACUMULACIÓN EN SESIÓN
                var productosAcumulados = new List<ProductoFactJsonDto>();
                int productosAgregados = 0;
                int erroresEncontrados = 0;
                var errores = new List<string>();
                int itemCorrelativo = 1; // ✅ NUEVO: Contador de items

                _logger?.LogInformation($"✅ Inicio de iteración de {cpf_nros.Count} pre-facturas");

                // ❻ ITERAR CADA PRE-FACTURA
                foreach (var cpf_nro in cpf_nros)
                {
                    if (string.IsNullOrWhiteSpace(cpf_nro))
                    {
                        _logger?.LogWarning($"⚠️ Pre-factura vacía, omitiendo...");
                        continue;
                    }

                    _logger?.LogInformation($"───────────────────────────────────────────────────");
                    _logger?.LogInformation($"🔄 Procesando pre-factura: {cpf_nro}");

                    try
                    {
                        // ❼ INVOCAR MÉTODO ENCAPSULADO (REUTILIZABLE)
                        var resultado = await ObtenerProductoDatosCommon(
                            tipoValor: "F",           // ✅ Tipo Pre-Factura
                            valor: cpf_nro.Trim().PadLeft(6, '0'),
                            listaPreciosId,
                            canalId,
                            identificadorCliente,
                            cantidad: 1,
                            bulto: true
                        );

                        // ❽ VALIDAR RESULTADO
                        if (resultado == null || !resultado.Ok)
                        {
                            erroresEncontrados++;
                            string mensajeError = resultado?.Mensaje ?? "Error desconocido";
                            errores.Add($"Pre-factura {cpf_nro}: {mensajeError}");
                            _logger?.LogWarning($"❌ Error en {cpf_nro}: {mensajeError}");
                            continue;
                        }

                        // ❾ ACUMULAR PRODUCTOS EN LISTA DE SESIÓN CON MAPEO COMPLETO
                        if (resultado.ListaEntidad != null && resultado.ListaEntidad.Count > 0)
                        {
                            foreach (var producto in resultado.ListaEntidad)
                            {
                                // ✅ VALIDAR QUE EL PRODUCTO SEA VÁLIDO (respuesta = 0)
                                if (producto.respuesta != 0)
                                {
                                    _logger?.LogWarning($"⚠️ Producto {producto.p_id} omitido: {producto.respuesta_msj}");
                                    continue;
                                }

                                // ✅ MAPEO COMPLETO: Convertir ProductoDatosDto a ProductoFactJsonDto
                                var productoJson = new ProductoFactJsonDto
                                {
                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 1: IDENTIFICACIÓN
                                    // ═══════════════════════════════════════════════════
                                    item = itemCorrelativo++, // ✅ CRÍTICO: Item correlativo
                                    p_id = producto.p_id ?? string.Empty,
                                    p_id_barrado = producto.p_id_barrado ?? string.Empty,
                                    p_desc = producto.p_desc ?? string.Empty,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 2: PRECIOS BASE
                                    // ═══════════════════════════════════════════════════
                                    p_pcosto = producto.p_pcosto,
                                    p_pcosto_repo = producto.p_pcosto_repo,
                                    p_pneto = producto.p_pneto,
                                    p_pvta = producto.p_pvta,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 3: CANTIDAD Y PRECIO TOTAL
                                    // ═══════════════════════════════════════════════════
                                    cantidad_tot = producto.cantidad_tot,
                                    p_pvta_tot = producto.p_pvta * producto.cantidad_tot,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 4: IVA
                                    // ═══════════════════════════════════════════════════
                                    iva_situacion = producto.iva_situacion ?? string.Empty,
                                    iva_alicuota = producto.iva_alicuota,
                                    p_iva = producto.p_iva,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 5: IMPUESTOS INTERNOS
                                    // ═══════════════════════════════════════════════════
                                    in_alicuota = producto.in_alicuota,
                                    p_in = producto.p_in,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 6: PRECIO DE OFERTA
                                    // ═══════════════════════════════════════════════════
                                    po = producto.po,
                                    po_limite = producto.po_limite,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 7: MÁRGENES (SI EL SP LOS RETORNA)
                                    // ═══════════════════════════════════════════════════
                                    p_margen_imp = 0,// producto.p_margen_imp??0,
                                    p_margen_vig = 0,// producto.p_margen_vig??0,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 8: TOTALES CALCULADOS POR COMPROBANTE
                                    // ═══════════════════════════════════════════════════
                                    cm_gravado = producto.cm_gravado,
                                    cm_no_gravado = producto.cm_no_gravado,
                                    cm_exento = producto.cm_exento,
                                    cm_iva = producto.cm_iva,
                                    cm_ii = producto.cm_ii,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 9: DESCUENTOS (OPCIONALES)
                                    // ═══════════════════════════════════════════════════
                                    cm_dto = producto.cm_dto,
                                    cm_dto_porc = producto.cm_dto_porc,

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 10: TRAZABILIDAD DE ORIGEN
                                    // ═══════════════════════════════════════════════════
                                    cta_id = identificadorCliente, // ✅ Cliente actual (no del producto)
                                    pre_id = producto.pre_id,
                                    cpf_nro = cpf_nro, // ✅ Código de pre-factura actual

                                    // ═══════════════════════════════════════════════════
                                    // ✅ SECCIÓN 11: COMBOS (SI APLICA)
                                    // ═══════════════════════════════════════════════════
                                    //cmb_p_id = producto.cmb_p_id ?? string.Empty,
                                    //cmb = producto.cmb ?? string.Empty,
                                    //cmb_id = producto.cmb_id,
                                    //cmb_dto = producto.cmb_dto,
                                    //cmb_cant = producto.cmb_cant,
                                    //cmb_desc = producto.cmb_desc
                                };

                                productosAcumulados.Add(productoJson);
                                productosAgregados++;

                                _logger?.LogInformation($"  ✅ Producto {itemCorrelativo - 1}: {producto.p_desc} (Cant: {producto.cantidad_tot})");
                            }
                        }
                        else
                        {
                            _logger?.LogWarning($"⚠️ Pre-factura {cpf_nro} no tiene productos");
                        }
                    }
                    catch (Exception exPrefactura)
                    {
                        erroresEncontrados++;
                        errores.Add($"Pre-factura {cpf_nro}: Error inesperado");
                        _logger?.LogError(exPrefactura, $"❌ Excepción al procesar {cpf_nro}");
                    }
                }

                // ❿ GUARDAR PRODUCTOS ACUMULADOS EN SESIÓN
                FacturaProductos = productosAcumulados;
                _logger?.LogInformation($"✅ Total productos en sesión: {productosAcumulados.Count}");

                stopwatch.Stop();
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation($"✅ PROCESO COMPLETADO");
                _logger?.LogInformation($"   Productos agregados: {productosAgregados}");
                _logger?.LogInformation($"   Errores encontrados: {erroresEncontrados}");
                _logger?.LogInformation($"   Tiempo total: {stopwatch.ElapsedMilliseconds}ms");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ⓫ RETORNAR RESPUESTA EN FORMATO ESPERADO POR JAVASCRIPT
                if (erroresEncontrados > 0 && productosAgregados == 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se pudieron cargar productos de ninguna pre-factura",
                        errores = errores,
                        productosAgregados = 0,
                        totalProductos = productosAcumulados.Count
                    });
                }

                return Json(new
                {
                    ok = true,
                    mensaje = erroresEncontrados > 0
                        ? $"Se cargaron {productosAgregados} productos con {erroresEncontrados} errores"
                        : $"Se cargaron {productosAgregados} productos exitosamente",
                    productosAgregados = productosAgregados,
                    totalProductos = productosAcumulados.Count,
                    errores = errores.Count > 0 ? errores : null,
                    producto = productosAcumulados // ✅ CRÍTICO: Mismo nombre que ObtenerProductoDatos
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "❌ EXCEPCIÓN en ObtenerProductosDatosPrefactura");
                _logger?.LogError($"   Tiempo antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al procesar pre-facturas. Por favor, intente nuevamente."
                });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Diferir Factura - Crea una pre-factura (factura diferida)
        /// Permite pausar la compra del cliente sin afectar el stock ni generar comprobante fiscal
        /// </summary>
        /// <returns>JSON con resultado de la operación</returns>
        [HttpPost]
        public async Task<JsonResult> DiferirFactura()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("💾 DIFERIR FACTURA - INICIO");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❷ VALIDAR DATOS DE CAJA
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogError("❌ No hay caja en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // ❸ VALIDAR DATOS DE CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogError("❌ No hay cliente en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado" });
                }

                // ❹ VALIDAR QUE HAYA PRODUCTOS
                var productosFactura = FacturaProductos;
                if (productosFactura == null || productosFactura.Count == 0)
                {
                    _logger?.LogWarning("❌ No hay productos en la factura");
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un producto" });
                }

                _logger?.LogInformation($"✅ Productos en factura: {productosFactura.Count}");

                // ❺ SERIALIZAR JSON DE PRODUCTOS
                string jsonProductos = JsonConvert.SerializeObject(productosFactura);

                _logger?.LogInformation($"✅ JSON de productos generado (longitud: {jsonProductos.Length})");

                // ❻ DETERMINAR IDENTIFICADOR DEL CLIENTE SEGÚN ORIGEN
                string ctaId;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    ctaId = clienteActual.cta_documento ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente CF → Identificador (documento): {ctaId}");
                }
                else // Cliente Registrado
                {
                    ctaId = clienteActual.cta_id ?? string.Empty;
                    _logger?.LogInformation($"✅ Cliente Registrado → Identificador (cta_id): {ctaId}");
                }

                // ❼ CONSTRUIR REQUEST DTO
                var request = new CajaPrefDiferidaReqDto
                {
                    Caja_Id = cajaActual.CajaId ?? string.Empty,
                    Usu_Id = UserName ?? string.Empty,
                    Adm_Id = cajaActual.AdmId ?? AdministracionId,
                    Lp_Id = LP_Id ?? string.Empty,
                    Caja_Nro_Proceso = cajaActual.Caja.caja_nro_proceso ?? string.Empty,
                    Caja_Nro_Cierre = cajaActual.Caja.caja_nro_cierre.ToInt(),
                    Cta_Id = ctaId,
                    Tdoc_Id = clienteActual.tdoc_id ?? string.Empty,
                    Cta_Documento = clienteActual.cta_documento ?? string.Empty,
                    Cta_Denominacion = clienteActual.cta_denominacion ?? string.Empty,
                    Sec_Id = "CAJA",
                    Json_P = jsonProductos
                };

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO:");
                _logger?.LogInformation($"   Caja_Id: {request.Caja_Id}");
                _logger?.LogInformation($"   Usu_Id: {request.Usu_Id}");
                _logger?.LogInformation($"   Adm_Id: {request.Adm_Id}");
                _logger?.LogInformation($"   Lp_Id: {request.Lp_Id}");
                _logger?.LogInformation($"   Caja_Nro_Proceso: {request.Caja_Nro_Proceso}");
                _logger?.LogInformation($"   Caja_Nro_Cierre: {request.Caja_Nro_Cierre}");
                _logger?.LogInformation($"   Cta_Id: {request.Cta_Id}");
                _logger?.LogInformation($"   Tdoc_Id: {request.Tdoc_Id}");
                _logger?.LogInformation($"   Cta_Documento: {request.Cta_Documento}");
                _logger?.LogInformation($"   Cta_Denominacion: {request.Cta_Denominacion}");
                _logger?.LogInformation($"   Sec_Id: {request.Sec_Id}");
                _logger?.LogInformation($"   Json_P (longitud): {request.Json_P.Length}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❽ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada" });
                }

                _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.CrearPrefacturaDiferida...");
                var resultado = await _productoFactServicio.CrearPrefacturaDiferida(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // ❾ VALIDAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al diferir la factura" });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error del servicio: {resultado.Mensaje}");
                    return Json(new { ok = false, mensaje = resultado.Mensaje ?? "Error al diferir la factura" });
                }

                // ❿ EXTRAER DATOS DE RESPUESTA
                var respuestaDto = resultado.Entidad;

                if (respuestaDto == null)
                {
                    _logger?.LogError("❌ No se recibió entidad de respuesta");
                    return Json(new { ok = false, mensaje = "Error: respuesta vacía del servidor" });
                }

                // ⓫ VALIDAR RESULTADO DEL SP
                if (respuestaDto.resultado != 0)
                {
                    _logger?.LogError($"❌ Error del SP: {respuestaDto.resultado_msj}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = respuestaDto.resultado_msj ?? "Error al crear la factura diferida"
                    });
                }

                // ⓬ ÉXITO - Formatear ID de prefactura
                string prefacturaId = respuestaDto.resultado_id ?? "DESCONOCIDO";

                // ✅ Formatear como '000256' (6 dígitos con ceros a la izquierda)
                if (int.TryParse(prefacturaId, out int idNumerico))
                {
                    prefacturaId = idNumerico.ToString("D6");
                }

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ FACTURA DIFERIDA CREADA EXITOSAMENTE");
                _logger?.LogInformation($"   ID: {prefacturaId}");
                _logger?.LogInformation($"   Mensaje: {respuestaDto.resultado_msj}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ⓭ LIMPIAR SESIÓN DE FACTURA (opcional, según requerimiento)
                // FacturaProductos = new List<ProductoFactJsonDto>();
                // FacturaSubtotales = new List<FactSubtotalJsonDto>();
                // FacturaSorteos = new List<object>();

                // ⓮ RETORNAR RESPUESTA
                return Json(new
                {
                    ok = true,
                    mensaje = $"Factura Diferida Creada '{prefacturaId}'",
                    prefactura_id = prefacturaId,
                    resultado_completo = respuestaDto.resultado_msj
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en DiferirFactura: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al diferir la factura. Por favor, intente nuevamente."
                });
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v10.0: Diferir Pago - Emite factura sin cobrar
        /// CORREGIDO: Parseo correcto de JSON en resultado_id
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> DiferirPago()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // ❶ VALIDAR AUTENTICACIÓN
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("⏱️ DIFERIR PAGO - INICIO v10.0");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❷ VALIDAR DATOS DE CAJA
                var cajaActual = CajaActual;
                if (cajaActual == null)
                {
                    _logger?.LogError("❌ No hay caja en sesión");
                    return Json(new { ok = false, mensaje = "No hay caja abierta" });
                }

                // ❸ VALIDAR DATOS DE CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogError("❌ No hay cliente en sesión");
                    return Json(new { ok = false, mensaje = "No hay cliente seleccionado" });
                }

                // ❹ VALIDAR QUE HAYA PRODUCTOS
                var productosFactura = FacturaProductos;
                if (productosFactura == null || productosFactura.Count == 0)
                {
                    _logger?.LogWarning("❌ No hay productos en la factura");
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un producto" });
                }

                // ❺ VALIDAR QUE HAYA SUBTOTALES
                var subtotalesFactura = FacturaSubtotales;
                if (subtotalesFactura == null || subtotalesFactura.Count == 0)
                {
                    _logger?.LogWarning("❌ No hay subtotales calculados");
                    return Json(new { ok = false, mensaje = "Debe calcular los totales primero" });
                }

                _logger?.LogInformation($"✅ Productos: {productosFactura.Count}");
                _logger?.LogInformation($"✅ Subtotales: {subtotalesFactura.Count}");

                // ❻ SERIALIZAR JSONs
                string jsonProductos = JsonConvert.SerializeObject(productosFactura);
                string jsonSubtotales = JsonConvert.SerializeObject(subtotalesFactura);

                var sorteosFactura = FacturaSorteos;
                string jsonSorteos = JsonConvert.SerializeObject(sorteosFactura);

                _logger?.LogInformation($"✅ JSON productos (longitud): {jsonProductos.Length}");
                _logger?.LogInformation($"✅ JSON subtotales (longitud): {jsonSubtotales.Length}");
                _logger?.LogInformation($"✅ JSON sorteos (longitud): {jsonSorteos.Length}");

                // ❼ DETERMINAR IDENTIFICADOR DEL CLIENTE SEGÚN ORIGEN
                string ctaId;
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "F") // Consumidor Final
                {
                    ctaId = clienteActual.cta_documento ?? string.Empty;
                    LP_Id = cajaActual.Caja.lp_id_min;
                    _logger?.LogInformation($"✅ Cliente CF → Identificador (documento): {ctaId}");
                }
                else // Cliente Registrado
                {
                    ctaId = clienteActual.cta_id ?? string.Empty;
                    LP_Id = cajaActual.Caja.lp_id_may;
                    _logger?.LogInformation($"✅ Cliente Registrado → Identificador (cta_id): {ctaId}");
                }

                // ❽ CONSTRUIR REQUEST DTO
                var request = new CajaOpeConfirmarReq
                {
                    // ═══ Datos de caja ═══
                    caja_id = cajaActual.CajaId ?? string.Empty,
                    usu_id = UserName ?? string.Empty,
                    adm_id = cajaActual.AdmId ?? AdministracionId,
                    lp_id = LP_Id ?? string.Empty,
                    caja_nro_proceso = cajaActual.Caja.caja_nro_proceso ?? string.Empty,
                    caja_nro_cierre = cajaActual.Caja.caja_nro_cierre.ToInt(),

                    // ═══ Datos de cliente ═══
                    cta_id = ctaId,
                    ctac_dto = clienteActual.ctac_dto_operacion,
                    ctc_id = clienteActual.ctc_id ?? string.Empty,

                    // ═══ CRÍTICO: Tipo de operación DIFERIR PAGO ═══
                    co_tipo = "DP",

                    // ═══ Datos de comprobante ═══
                    tco_letra = clienteActual.tco_letra ?? string.Empty,
                    tco_id_ori = string.Empty,
                    cm_compte_ori = string.Empty,

                    // ═══ Datos fiscales ═══
                    afip_id = clienteActual.afip_id ?? string.Empty,
                    tdoc_id = clienteActual.tdoc_id ?? string.Empty,
                    cta_documento = clienteActual.cta_documento ?? string.Empty,
                    cta_denominacion = clienteActual.cta_denominacion ?? string.Empty,
                    cta_domicilio = clienteActual.cta_domicilio ?? string.Empty,

                    // ═══ Vendedor (opcional) ═══
                    ve_id = string.Empty,

                    // ═══ JSONs de operación ═══
                    json_p = jsonProductos,
                    json_subtotal = jsonSubtotales,
                    json_sorteo = jsonSorteos,

                    // ═══ CRÍTICO: JSONs de pago VACÍOS ═══
                    json_valores = "{}",
                    json_cancela = "{}",
                    json_union = "{}"
                };

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 REQUEST DTO CONSTRUIDO");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❾ INVOCAR SERVICIO
                var token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogError("❌ No hay token de autenticación");
                    return Json(new { ok = false, mensaje = "Sesión expirada" });
                }

                _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.CrearDiferirPago...");
                var resultado = await _productoFactServicio.CrearDiferirPago(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // ❿ VALIDAR RESPUESTA
                if (resultado == null)
                {
                    _logger?.LogError("❌ El servicio retornó null");
                    return Json(new { ok = false, mensaje = "Error al diferir el pago" });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error del servicio: {resultado.Mensaje}");
                    return Json(new { ok = false, mensaje = resultado.Mensaje ?? "Error al diferir el pago" });
                }

                // ⓫ EXTRAER DATOS DE RESPUESTA
                var respuestaDto = resultado.Entidad;

                if (respuestaDto == null)
                {
                    _logger?.LogError("❌ No se recibió entidad de respuesta");
                    return Json(new { ok = false, mensaje = "Error: respuesta vacía del servidor" });
                }

                // ⓬ VALIDAR RESULTADO DEL SP
                if (respuestaDto.resultado != 0)
                {
                    _logger?.LogError($"❌ Error del SP: {respuestaDto.resultado_msj}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = respuestaDto.resultado_msj ?? "Error al emitir la factura diferida"
                    });
                }

                // ═══════════════════════════════════════════════════════════
                // ⓭ ✅ NUEVO v10.0: PARSEAR JSON DE COMPROBANTE CORRECTAMENTE
                // ═══════════════════════════════════════════════════════════

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 PARSEANDO DATOS DEL COMPROBANTE");
                _logger?.LogInformation($"   resultado_id raw: {respuestaDto.resultado_id}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ INTENTAR PARSEAR JSON
                if (!TryParsearComprobanteJson(respuestaDto.resultado_id, out var comprobante))
                {
                    _logger?.LogError("❌ No se pudo parsear resultado_id como JSON");

                    return Json(new
                    {
                        ok = false,
                        mensaje = "Error al procesar datos del comprobante. Formato inválido.",
                        debug_resultado_id = respuestaDto.resultado_id
                    });
                }

                // ❷ VALIDAR QUE EL COMPROBANTE SEA VÁLIDO
                if (comprobante == null)
                {
                    _logger?.LogError("❌ Comprobante es null después del parseo");
                    return Json(new { ok = false, mensaje = "Error: no se obtuvieron datos del comprobante" });
                }

                // ❸ LOGS DE DATOS PARSEADOS
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ FACTURA DIFERIDA EMITIDA EXITOSAMENTE");
                _logger?.LogInformation($"   Letra: {comprobante.tco_letra}");
                _logger?.LogInformation($"   ID Tipo: {comprobante.tco_id}");
                _logger?.LogInformation($"   Número: {comprobante.cm_compte}");
                _logger?.LogInformation($"   Repetido: {(comprobante.EsRepetido ? "SÍ" : "NO")}");
                _logger?.LogInformation($"   Mensaje: {respuestaDto.resultado_msj}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ⓮ LIMPIAR SESIÓN DE FACTURA
                FacturaProductos = new List<ProductoFactJsonDto>();
                FacturaSubtotales = [];
                FacturaSorteos = [];

                // ⓯ ✅ RETORNAR RESPUESTA CORRECTA PARA FRONTEND
                return Json(new
                {
                    ok = true,
                    mensaje = $"Factura {comprobante.tco_letra} Nro {comprobante.cm_compte} emitida con pago diferido",

                    // ✅ DATOS DEL COMPROBANTE EN FORMATO CORRECTO
                    data = new[]
                    {
                new
                {
                    tco_letra = comprobante.tco_letra,
                    tco_id = comprobante.tco_id,
                    cm_compte = comprobante.cm_compte,
                    cm_repetido = comprobante.cm_repetido
                }
            },

                    resultado_completo = respuestaDto.resultado_msj,
                    debe_imprimir = true
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError($"❌ EXCEPCIÓN en DiferirPago: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                _logger?.LogError($"   Tiempo antes del error: {stopwatch.ElapsedMilliseconds}ms");

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al diferir el pago. Por favor, intente nuevamente."
                });
            }
        }


        // ═══════════════════════════════════════════════════
        // ✅ NUEVO ENDPOINT v10.0 (CORREGIDO)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Obtiene la configuración de reportes disponibles
        /// </summary>
        [HttpGet("ObtenerConfigReportes")]
        public IActionResult ObtenerConfigReportes()
        {
            try
            {
                _logger.LogInformation("📋 Solicitando configuración de reportes...");

                var reportes = _reportesConfigService.ObtenerTodos();

                if (reportes == null || reportes.Count == 0)
                {
                    _logger.LogWarning("⚠️ No hay reportes configurados en appsettings.json");
                    return Ok(new
                    {
                        ok = false,
                        mensaje = "No hay reportes configurados"
                    });
                }

                _logger.LogInformation($"✅ Devolviendo {reportes.Count} reportes configurados");

                return Ok(new
                {
                    ok = true,
                    reportes = reportes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener configuración de reportes");
                return StatusCode(500, new
                {
                    ok = false,
                    mensaje = "Error al obtener configuración de reportes"
                });
            }
        }

        /// <summary>
        /// ✅ NUEVO v10.0: Genera un reporte de comprobante (Factura A, B, etc.)
        /// Usa FeReqDto existente en lugar de crear DTO nuevo
        /// </summary>
        /// <param name="request">Datos del comprobante (tco_letra, tco_id, cm_compte, cm_repetido)</param>
        /// <returns>PDF en Base64</returns>
        [HttpPost("GenerarReporteComprobante")]
        public async Task<IActionResult> GenerarReporteComprobante([FromBody] FeReqDto request)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("📄 GENERAR REPORTE DE COMPROBANTE v10.0");
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"   tco_letra: {request.tco_letra}");
                _logger.LogInformation($"   tco_id: {request.tco_id}");
                _logger.LogInformation($"   cm_compte: {request.cm_compte}");
                _logger.LogInformation($"   cm_repetido: {request.cm_repetido}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                // ❶ Validar entrada
                if (string.IsNullOrWhiteSpace(request.tco_letra))
                {
                    return Ok(new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Debe especificar la letra del comprobante (tco_letra)",
                        Base64 = string.Empty
                    });
                }

                // ❷ Obtener configuración del reporte según la letra
                var reporteConfig = _reportesConfigService.ObtenerPorKey(request.tco_letra);

                if (reporteConfig == null)
                {
                    _logger.LogWarning($"⚠️ No existe configuración para comprobante tipo '{request.tco_letra}'");
                    return Ok(new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = $"No se encuentra configurado el reporte para comprobante tipo '{request.tco_letra}'",
                        Base64 = string.Empty
                    });
                }

                _logger.LogInformation($"✅ Reporte identificado: {reporteConfig.Nombre} (ID: {reporteConfig.Id})");

                // ❸ Construir solicitud para API de reportes
                var reporteSolicitud = new ReporteSolicitudDto
                {
                    Reporte = (InfoReporte)int.Parse(reporteConfig.Id),
                    Parametros = new Dictionary<string, string>
            {
                { "tco_id", request.tco_id ?? string.Empty },
                { "cm_compte", request.cm_compte ?? string.Empty },
                { "cm_repetido", request.cm_repetido ?? "0" }
            },
                    Titulo = reporteConfig.Nombre,
                    Formato = "P" // PDF
                };

                _logger.LogInformation($"📡 Invocando API de Reportes con ID: {reporteConfig.Id}");

                // ❹ Obtener token de autenticación
                var token = HttpContext.Session.GetString("TKN");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No se encontró token de sesión");
                    return Ok(new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Sesión expirada. Por favor, inicie sesión nuevamente.",
                        Base64 = string.Empty
                    });
                }

                // ❺ Llamar a la API de reportes usando el servicio extraído
                var respuestaReporte = await _reportesService.ObtenerPdfDesdeAPI(reporteSolicitud, token);

                if (respuestaReporte.resultado != 0)
                {
                    _logger.LogError($"❌ Error en API de Reportes: {respuestaReporte.resultado_msj}");
                    return Ok(respuestaReporte);
                }

                if (string.IsNullOrWhiteSpace(respuestaReporte.Base64))
                {
                    _logger.LogError("❌ La API de Reportes no devolvió contenido Base64");
                    return Ok(new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "El reporte se generó pero no se obtuvo contenido",
                        Base64 = string.Empty
                    });
                }

                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"✅ REPORTE GENERADO EXITOSAMENTE");
                _logger.LogInformation($"   Tamaño Base64: {respuestaReporte.Base64.Length} caracteres");
                _logger.LogInformation($"   Nombre archivo: {respuestaReporte.resultado_msj}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                return Ok(respuestaReporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar reporte de comprobante");
                return StatusCode(500, new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "Error interno al generar el reporte",
                    Base64 = string.Empty
                });
            }
        }

        /// <summary>
        /// ✅ NUEVO v10.0: Parsea el JSON de comprobante desde resultado_id
        /// </summary>
        /// <param name="resultadoId">JSON string con información del comprobante</param>
        /// <param name="comprobante">DTO con datos parseados (out)</param>
        /// <returns>true si el parseo fue exitoso, false en caso contrario</returns>
        private bool TryParsearComprobanteJson(string resultadoId, out ComprobanteInfoDto? comprobante)
        {
            comprobante = null;

            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 PARSEANDO JSON DE COMPROBANTE v10.0");
                _logger?.LogInformation($"   JSON recibido: {resultadoId}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ VALIDAR QUE NO SEA NULL O VACÍO
                if (string.IsNullOrWhiteSpace(resultadoId))
                {
                    _logger?.LogWarning("❌ resultado_id es null o vacío");
                    return false;
                }

                // ❷ LIMPIAR JSON (remover espacios)
                string jsonLimpio = resultadoId.Trim();

                // ❸ VALIDAR QUE SEA UN ARRAY JSON
                if (!jsonLimpio.StartsWith("[") || !jsonLimpio.EndsWith("]"))
                {
                    _logger?.LogWarning($"⚠️ El JSON no es un array válido: {jsonLimpio}");
                    return false;
                }

                // ❹ DESERIALIZAR COMO LISTA
                var lista = JsonConvert.DeserializeObject<List<ComprobanteInfoDto>>(jsonLimpio);

                // ❺ VALIDAR QUE LA LISTA NO SEA NULL Y TENGA AL MENOS UN ELEMENTO
                if (lista == null || lista.Count == 0)
                {
                    _logger?.LogWarning("❌ La deserialización retornó lista vacía o null");
                    return false;
                }

                // ❻ TOMAR EL PRIMER ELEMENTO (normalmente será único)
                comprobante = lista[0];

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ COMPROBANTE PARSEADO EXITOSAMENTE");
                _logger?.LogInformation($"   tco_letra: {comprobante.tco_letra}");
                _logger?.LogInformation($"   tco_id: {comprobante.tco_id}");
                _logger?.LogInformation($"   cm_compte: {comprobante.cm_compte}");
                _logger?.LogInformation($"   cm_repetido: {comprobante.cm_repetido} ({(comprobante.EsRepetido ? "SÍ" : "NO")})");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return true;
            }
            catch (JsonException ex)
            {
                _logger?.LogError($"❌ ERROR DE PARSEO JSON: {ex.Message}");
                _logger?.LogError($"   JSON problemático: {resultadoId}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ ERROR INESPERADO AL PARSEAR: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }

    // Necesitaremos recibir lista de códigos de pre-factura
    public class PrefacturaProductosReqDto
    {
        public List<string> cpf_nros { get; set; } = [];
    }

    //public class ProductoDatosResponseDto
    //{
    //    public bool Ok { get; set; } = false;
    //    public string Mensaje { get; set; } = "Parámetros inválidos";
    //}
}
