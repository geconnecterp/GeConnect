using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
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

        public ProductoFactController(
            IOptions<AppSettings> options,
            ICajaServicio cajaServicio,
            IProductoFactServicio productoFactServicio, // ✅ INYECTAR
            IHttpContextAccessor httpContext,
            ILogger<ProductoFactController> logger) : base(options, httpContext, logger)
        {
            _cajaServicio = cajaServicio;
            _productoFactServicio = productoFactServicio; // ✅ ASIGNAR
        }

        public IActionResult Index()
        {
            return View();
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
                    ctac_dto = clienteActual.ctac_dto_operacion
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
                    return Json(new { ok = false, mensaje = "Sesión expirada. Por favor, vuelva a iniciar sesión." });
                }

                _logger?.LogInformation("📡 Invocando servicio ProductoFactServicio.ObtenerProductoDatos...");
                var resultado = await _productoFactServicio.ObtenerProductoDatos(request, token);

                stopwatch.Stop();
                _logger?.LogInformation($"⏱️ Tiempo de ejecución: {stopwatch.ElapsedMilliseconds}ms");

                // ❾ PROCESAR RESPUESTA
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

        //private void InicializaVariablesBusquedaBase()
        //{
        //    #region Variables de InfoProd
        //    InfoProdStkDId = "";
        //    InfoProdStkDRegs = [];
        //    InfoProdStkBoxesIds = ("", "");
        //    InfoProdStkBoxesRegs = [];
        //    InfoProdStkAId = "";
        //    InfoProdStkARegs = [];
        //    InfoProdMovStkIds = "";
        //    InfoProdMovStkRegs = [];
        //    InfoProdLPId = "";
        //    InfoProdLPRegs = [];

        //    #endregion
        //}


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
                _logger?.LogInformation("📋 REQUEST COMPLETO:");
                _logger?.LogInformation($"   caja_id: {request.caja_id}");
                _logger?.LogInformation($"   usu_id: {request.usu_id}");
                _logger?.LogInformation($"   adm_id: {request.adm_id}");
                _logger?.LogInformation($"   lp_id: {request.lp_id}");
                _logger?.LogInformation($"   cta_id: {request.cta_id}");
                _logger?.LogInformation($"   ctac_dto: {request.ctac_dto}");
                _logger?.LogInformation($"   ctc_id: {request.ctc_id}");
                _logger?.LogInformation($"   afip_id: {request.afip_id}");
                _logger?.LogInformation($"   tot_rows: {request.tot_rows}");
                _logger?.LogInformation($"   tot_cantidad: {request.tot_cantidad}");
                _logger?.LogInformation($"   tot_pvta: {request.tot_pvta}");
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
        /// ✅ NUEVO: Obtiene las pre-facturas disponibles para un cliente
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerPreFacturas(string cta_id, bool solo_pendientes = true)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                if (string.IsNullOrEmpty(cta_id))
                {
                    return Json(new { ok = false, mensaje = "Debe especificar un cliente" });
                }

                // TODO: Implementar servicio
                // var resultado = await _productoFactServicio.ObtenerPreFacturas(cta_id, solo_pendientes, TokenCookie);

                // MOCK para desarrollo:
                var prefacturas = new[]
                {
            new
            {
                pre_id = "000888",
                cta_denominacion = "Roberto Fulano",
                cta_documento = "25147852",
                pre_fecha = "15/01/26 10:40",
                sector_desc = "Perfumeria"
            },
            new
            {
                pre_id = "000889",
                cta_denominacion = "Roberto Fulano",
                cta_documento = "25147852",
                pre_fecha = "15/01/26 10:55",
                sector_desc = "Perfumeria"
            }
        };

                return Json(new
                {
                    ok = true,
                    prefacturas = prefacturas
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener pre-facturas");
                return Json(new { ok = false, mensaje = "Error al obtener pre-facturas" });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Obtiene las cotizaciones disponibles para un cliente
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerCotizaciones(string cta_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                if (string.IsNullOrEmpty(cta_id))
                {
                    return Json(new { ok = false, mensaje = "Debe especificar un cliente" });
                }

                // TODO: Implementar servicio
                // var resultado = await _productoFactServicio.ObtenerCotizaciones(cta_id, TokenCookie);

                // MOCK para desarrollo:
                var cotizaciones = new[]
                {
            new
            {
                cpf_nro = "000888",
                cpf_descripcion = "Licitación 1889-25",
                cpf_fecha = "10/02/26",
                obs_pago = "Pago de Contado",
                cpf_importe = 455500.00
            }};

                return Json(new
                {
                    ok = true,
                    cotizaciones = cotizaciones
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener cotizaciones");
                return Json(new { ok = false, mensaje = "Error al obtener cotizaciones" });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Busca pre-facturas según filtros
        /// Implementa el requerimiento del 23 de abril
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BuscarPrefacturas(string cta_id, string documento, bool solo_pendientes = true)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "Sesión expirada" });

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔍 BUSCAR PRE-FACTURAS - CONTROLLER");
                _logger?.LogInformation($"   cta_id: {cta_id}");
                _logger?.LogInformation($"   documento: {documento}");
                _logger?.LogInformation($"   solo_pendientes: {solo_pendientes}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ VALIDAR CLIENTE
                var clienteActual = ClienteActual;
                if (clienteActual == null)
                {
                    _logger?.LogWarning("❌ No hay cliente seleccionado");
                    return Json(new { ok = false, mensaje = "Debe seleccionar un cliente primero" });
                }

                // ❷ CONSTRUIR REQUEST SEGÚN TIPO DE CLIENTE
                var request = new PrefacturaReqDto
                {
                    sec_id = "CAJA", // ✅ FIJO según requerimiento
                    usada = solo_pendientes ? "N" : "%" // ✅ Checkbox: N (pendientes) o % (todos)
                };

                // ✅ LÓGICA DIFERENCIAL: Cliente registrado vs Consumidor Final
                string origenUpper = clienteActual.Origen?.ToUpper() ?? "F";

                if (origenUpper == "C") // Cliente Registrado
                {
                    request.cta_id = clienteActual.cta_id ?? cta_id ?? "%";
                    request.documento = "%"; // ✅ Anular documento
                    _logger?.LogInformation($"✅ Cliente registrado → cta_id: {request.cta_id}");
                }
                else // Consumidor Final
                {
                    request.cta_id = "%"; // ✅ Anular cta_id
                    request.documento = clienteActual.cta_documento ?? documento ?? "%";
                    _logger?.LogInformation($"✅ Consumidor final → documento: {request.documento}");
                }

                // ❸ INVOCAR SERVICIO
                var resultado = await _productoFactServicio.ObtenerPrefactura(request, TokenCookie);

                if (resultado == null || !resultado.Ok)
                {
                    _logger?.LogWarning($"⚠️ Error al buscar: {resultado?.Mensaje}");
                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado?.Mensaje ?? "Error al buscar pre-facturas",
                        prefacturas = new List<PrefacturaResDto>()
                    });
                }

                _logger?.LogInformation($"✅ Se encontraron {resultado.ListaEntidad?.Count ?? 0} pre-facturas");

                return Json(new
                {
                    ok = true,
                    mensaje = "OK",
                    prefacturas = resultado.ListaEntidad ?? new List<PrefacturaResDto>()
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar pre-facturas");
                return Json(new { ok = false, mensaje = "Error al buscar pre-facturas", prefacturas = new List<PrefacturaResDto>() });
            }
        }

        /// <summary>
        /// ✅ NUEVO: Busca cotizaciones para un cliente
        /// Implementa el requerimiento del 23 de abril
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BuscarCotizaciones(string cta_id)
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
                    cotizaciones = resultado.ListaEntidad ?? new List<CotizacionResDto>()
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar cotizaciones");
                return Json(new { ok = false, mensaje = "Error al buscar cotizaciones", cotizaciones = new List<CotizacionResDto>() });
            }
        }
    }
}
