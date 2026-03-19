using Azure;
using DocumentFormat.OpenXml.Office.CustomUI;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ORCtlController : PocketControllerBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly IORServicio _orServicio;
        public ORCtlController(IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ILogger<TrIntController> logger,
            IORServicio oRServicio,
            IOptions<MenuSettings> options1) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _orServicio = oRServicio;
        }



        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
            //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
            var sigla = "CTL-OR";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            ORSession = new ORSessionDto();

            return View();
        }

        /// <summary>
        /// Basicamente presenta la vista con el grid de productos de la OR Controlada. 
        /// Para esto necesita recibir el numero de comprobante de la OR, que es lo que 
        /// identifica a la orden de reparto y a su vez a los productos que contendrá.
        /// </summary>
        /// <param name="or_compte"></param>
        /// <returns></returns>
        /// <exception cref="NegocioException"></exception>
        [HttpGet]
        public IActionResult PresentaProductosOrCtl(string or_compte)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(or_compte))
            {
                TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }

            // ✅ REFACTORIZADO: Usar ORSession
            //la inicializacion con nuevo comprobante
            var session = ORSession;
            if (string.IsNullOrEmpty(session.ORComprobanteActual))
            {
                session.ORComprobanteActual = or_compte;
                session.UltimaActualizacion = DateTime.Now;
                ORSession = session;
            }

            _logger?.LogInformation("📝 OR Seleccionada: {OrCompte}", or_compte);

            var sigla = "CTL-OR";
            string? volver = Url.Action("index", "orctl", new { area = "PocketPpal" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = session.ORComprobanteActual;

            return View();
        }


        /// <summary>
        /// Realiza la carga inicial desde el server para poder seguir cargando productos,
        /// de ser necesario.
        /// </summary>
        /// <param name="or_compte"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CargaProductosOrCtl(string or_compte)
        {
            try
            {
                if (string.IsNullOrEmpty(or_compte))
                {
                    TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                    return RedirectToAction("index");
                }

                var sesion = ORSession;

                //si la lista esta vacía o nula, la cargo desde el servicio,
                //\\sino, dejo la que ya esta en la sesión para seguir cargando productos
                //\\sin perder lo que ya se tenia cargado
                if (sesion == null || (sesion?.ORCtlListaProductos == null || sesion.ORCtlListaProductos.Count() == 0))
                {
                    if (sesion == null)
                    {
                        sesion = new ORSessionDto();
                        sesion.ORComprobanteActual = or_compte;
                    }
                    if (sesion.ORCtlListaProductos == null)
                    {
                        sesion.ORCtlListaProductos = new();
                    }

                    var prod = await _orServicio.ObtenerListaProductosOrCtl(or_compte, UserName, TokenCookie);

                    int cant = 0;
                    foreach (var p in prod.ListaEntidad ?? [])
                    {
                        cant++;
                        sesion.ORCtlListaProductos.Add(new OrCtlCargaProductoDto
                        {
                            bulto = p.bultos,
                            cantidad = p.cantidad,
                            or_compte = p.or_compte,
                            p_desc = p.p_desc,
                            p_id = p.p_id,
                            up_id = p.up_id,
                            item = cant,
                            p_id_barrado = p.p_id_barrado,
                            p_id_prov = p.p_id_prov,
                            unidad_pres = p.unidad_pres,
                            us = p.us,
                            usu_id = UserName,
                            vto = p.vto.ToString("yyyyMMdd")
                        });
                    }

                    //cargamos los productos en la sesion recuperando los registros 
                    //(ninguno o los ya cargados desde la base de datos)
                    sesion.UltimaActualizacion = DateTime.Now;
                    ORSession = sesion;
                }
                return Json(new { success = true, message = "", data = sesion.ORCtlListaProductos });

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al intentar obtener los productos");
                return Json(new { success = false, message = "Hubo algún tipo de error y no se pudo obtener los Productos. " });
            }
        }


        /// <summary>
        /// Vista que se encarga de buscar y cargar la información del producto 
        /// que se quiere agregar al carrito de la OR de Control.
        /// </summary>
        /// <param name="or_compte"></param>
        /// <param name="p_id"></param>
        /// <param name="nuevo"></param>
        /// <returns></returns>
        /// <exception cref="NegocioException"></exception>
        [HttpGet]
        public IActionResult ORValidaProducto(string or_compte, string p_id, bool nuevo = false)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(p_id) && !nuevo)
            {
                TempData["error"] = "No se recepcionó el ID del Producto.";
                return RedirectToAction("ORCargaCarrito");
            }

            // Configurar ViewBag
            var sigla = "CTL-OR";
            string? volver = Url.Action("PresentaProductosOrCtl", "orctl",
                new { area = "PocketPpal", or_compte });

            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x =>
                x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo.");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = or_compte;

            return View((string.Empty, or_compte));
        }

        //public async Task<IActionResult> ResguardarProductoCarritoORCtl(string p_id, int up, int bulto, decimal unid, decimal cantidad, DateTime? fv)
        //{

        //    if (string.IsNullOrEmpty(p_id))
        //    {
        //        TempData["error"] = "No se recepcionó el ID del Producto.";
        //        return RedirectToAction("ORValidaProducto");
        //    }
        //    // Aquí iría la lógica para resguardar el producto en el carrito, utilizando _orServicio.ResguardarProductoCarrito
        //    // Por ahora, redirigimos de vuelta a la vista de validación
        //    return RedirectToAction("ORValidaProducto", new { or_compte = ORSession?.ORComprobanteActual, p_id });
        //}

        [HttpPost]
        public IActionResult ResguardarProductoCarritoORCtl([FromBody] OrCtlCargaProductoDto request)
        {
            try
            {
                // ===================================================================
                // PASO 1: VALIDACIONES BÁSICAS
                // ===================================================================

                if (request == null)
                {
                    return Json(new { error = true, warn = false, msg = "No se recibieron datos del producto." });
                }

                if (string.IsNullOrWhiteSpace(request.p_id))
                {
                    return Json(new { error = true, warn = false, msg = "No se especificó el producto a cargar." });
                }

                if (request.cantidad <= 0)
                {
                    return Json(new { error = false, warn = true, msg = "Las cantidades de los productos a cargar deben ser positivas, mayores a 0 (cero)." });
                }

                // Validar unidad de presentación para productos pesables (UP_ID != "07")
                if (!string.IsNullOrWhiteSpace(request.up_id) && !request.up_id.Equals("07") && request.unidad_pres != 1)
                {
                    return Json(new { error = false, warn = true, msg = "El producto no es por unidades. La unidad de presentación tiene que ser igual a 1 siempre." });
                }

                // ===================================================================
                // PASO 2: COMPLETAR CAMPOS FALTANTES
                // ===================================================================

                if (string.IsNullOrWhiteSpace(request.or_compte))
                {
                    request.or_compte = ORSession?.ORComprobanteActual ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(request.or_compte))
                {
                    return Json(new { error = true, warn = false, msg = "No se especificó el número de comprobante de la OR." });
                }

                if (string.IsNullOrWhiteSpace(request.vto))
                {
                    request.vto = "19700101"; // Fecha por defecto
                }

                // ===================================================================
                // PASO 3: VALIDAR SESIÓN
                // ===================================================================

                var sesion = ORSession;
                if (sesion == null)
                {
                    return Json(new { error = true, warn = false, msg = "Sesión inválida. Por favor, inicie sesión nuevamente." });
                }

                // Inicializar lista si es null
                if (sesion.ORCtlListaProductos == null)
                {
                    sesion.ORCtlListaProductos = new List<OrCtlCargaProductoDto>();
                }

                // ===================================================================
                // PASO 4: VERIFICAR SI EL PRODUCTO YA EXISTE (DUPLICADO)
                // ===================================================================

                var productoExistente = BuscarProductoEnLista(sesion.ORCtlListaProductos, request.p_id);
                bool esActualizacion = productoExistente != null;

                if (esActualizacion)
                {
                    // ===================================================================
                    // CASO A: PRODUCTO DUPLICADO - ACTUALIZAR
                    // ===================================================================

                    _logger?.LogInformation("🔄 Producto duplicado detectado: {PId}. Actualizando datos...", request.p_id);

                    // Actualizar el producto existente con los nuevos datos
                    ActualizarProductoExistente(productoExistente, request);

                    // Actualizar sesión
                    sesion.UltimaActualizacion = DateTime.Now;
                    ORSession = sesion;

                    var mensajeActualizacion = $"Producto {request.p_desc ?? request.p_id} actualizado correctamente";

                    _logger?.LogInformation("✅ Producto actualizado exitosamente: {PId}", request.p_id);
                    TempData["succ"] = mensajeActualizacion;

                    return Json(new
                    {
                        error = false,
                        warn = false,
                        msg = mensajeActualizacion,
                        data = new
                        {
                            esActualizacion = true,
                            productoActualizado = productoExistente,
                            totalProductos = sesion.ORCtlListaProductos.Count
                        }
                    });
                }
                else
                {
                    // ===================================================================
                    // CASO B: PRODUCTO NUEVO - AGREGAR
                    // ===================================================================

                    _logger?.LogInformation("➕ Producto nuevo: {PId}. Agregando a la lista...", request.p_id);

                    // Calcular nuevo item (siguiente número secuencial)
                    request.item = sesion.ORCtlListaProductos.Count + 1;

                    // ✅ CORRECCIÓN: ASIGNAR USUARIO A TODOS LOS PRODUCTOS
                    request.usu_id = UserName;
                    _logger?.LogDebug("👤 Asignando usuario {Usuario} al producto item {Item}", UserName, request.item);

                    // Agregar a la lista
                    sesion.ORCtlListaProductos.Add(request);

                    // Actualizar sesión
                    sesion.UltimaActualizacion = DateTime.Now;
                    ORSession = sesion;

                    var mensajeNuevo = $"Producto {request.p_desc ?? request.p_id} agregado correctamente";

                    _logger?.LogInformation("✅ Producto agregado exitosamente: {PId} - Total: {Total}",
                        request.p_id, sesion.ORCtlListaProductos.Count);
                    TempData["succ"] = mensajeNuevo;

                    return Json(new
                    {
                        error = false,
                        warn = false,
                        msg = mensajeNuevo,
                        data = new
                        {
                            esActualizacion = false,
                            productoAgregado = request,
                            totalProductos = sesion.ORCtlListaProductos.Count
                        }
                    });
                }
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "❌ Error de negocio al cargar producto OR Control: {Message}", ex.Message);
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "❌ Error de autorización al cargar producto OR Control");
                return Json(new { error = false, warn = true, msg = "No tiene permisos para realizar esta operación." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al cargar producto OR Control");
                return Json(new { error = true, warn = false, msg = "Ocurrió un error inesperado. Intente nuevamente." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReguardaProductosEnServerOrCtl()
        {
            try
            {
                var sesion = ORSession;
                if (sesion == null || string.IsNullOrEmpty(sesion.ORComprobanteActual))
                {
                    return Json(new { success = false, message = "No se encontró una sesión válida para guardar los productos." });
                }

                // Serializar el request completo a JSON
                var jsonRequest = JsonConvert.SerializeObject(sesion.ORCtlListaProductos);

                _logger?.LogInformation("📦 Cargando productos OR Control:Cantidad: {Cantidad}", sesion.ORCtlListaProductos.Count());

                // Invocar servicio de carga de producto controlado
                var resp = await _orServicio.CargaProductoORCtl(jsonRequest, TokenCookie);

                if (!resp.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al cargar productos: {Mensaje}", resp.Mensaje);
                    return Json(new { error = resp.EsError, warn = resp.EsWarn, msg = resp.Mensaje ?? "Error al cargar el producto." });
                }

                // Respuesta exitosa
                return Json(new { success = true, message = "Productos guardados exitosamente en el servidor." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al guardar productos en el servidor");
                return Json(new { success = false, message = "Ocurrió un error inesperado al guardar los productos. Intente nuevamente." });
            }
        }

        /// <summary>
        /// Elimina un producto de la lista de productos OR Control en sesión.
        /// </summary>
        /// <param name="p_id">ID del producto a eliminar</param>
        /// <returns>JSON con resultado de la operación</returns>
        [HttpPost]
        public IActionResult EliminarProductoOrCtl(string p_id)
        {
            try
            {
                // Validación de parámetros
                if (string.IsNullOrWhiteSpace(p_id))
                {
                    return Json(new { error = false, warn = true, msg = "Debe especificar el ID del producto a eliminar." });
                }

                // Validar sesión
                var sesion = ORSession;
                if (sesion == null || sesion.ORCtlListaProductos == null || sesion.ORCtlListaProductos.Count == 0)
                {
                    return Json(new { error = false, warn = true, msg = "No hay productos cargados en la sesión." });
                }

                // Buscar el producto a eliminar
                var productoAEliminar = sesion.ORCtlListaProductos.FirstOrDefault(p => p.p_id.Equals(p_id, StringComparison.OrdinalIgnoreCase));

                if (productoAEliminar == null)
                {
                    return Json(new { error = false, warn = true, msg = $"No se encontró el producto {p_id} en la lista." });
                }

                // Guardar descripción para el mensaje
                var descripcionProducto = productoAEliminar.p_desc ?? p_id;

                // Eliminar el producto de la lista
                sesion.ORCtlListaProductos.Remove(productoAEliminar);

                // Reindexar items (ahora asigna usuario a todos)
                ReindexarProductosOrCtl(sesion.ORCtlListaProductos);

                // Log informativo
                if (sesion.ORCtlListaProductos.Count == 0)
                {
                    _logger?.LogInformation("📝 Lista de productos vacía después de eliminar {PId}", p_id);
                }

                // Actualizar sesión
                sesion.UltimaActualizacion = DateTime.Now;
                ORSession = sesion;

                _logger?.LogInformation("✅ Producto eliminado: {PId} - Quedan {Cantidad} productos", p_id, sesion.ORCtlListaProductos.Count);

                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = $"Producto {descripcionProducto} eliminado correctamente.",
                    data = new
                    {
                        productosRestantes = sesion.ORCtlListaProductos.Count,
                        productos = sesion.ORCtlListaProductos
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al eliminar producto OR Control");
                return Json(new { error = true, warn = false, msg = "Ocurrió un error inesperado al eliminar el producto." });
            }
        }

        /// <summary>
        /// Reindexar los items de la lista de productos después de una eliminación
        /// para mantener la secuencia correcta (1, 2, 3, ...)
        /// </summary>
        /// <param name="listaProductos">Lista de productos a reindexar</param>
        private void ReindexarProductosOrCtl(List<OrCtlCargaProductoDto> listaProductos)
        {
            if (listaProductos == null || listaProductos.Count == 0)
                return;

            for (int i = 0; i < listaProductos.Count; i++)
            {
                listaProductos[i].item = i + 1;

                if (string.IsNullOrWhiteSpace(listaProductos[i].usu_id))
                {
                    listaProductos[i].usu_id = UserName;
                    _logger?.LogDebug("👤 Usuario asignado al item {Item}", i + 1);
                }
            }

            _logger?.LogDebug("🔄 Productos reindexados: {Cantidad} items", listaProductos.Count);
        }

        /// <summary>
        /// Actualiza los datos de un producto existente con nueva información
        /// </summary>
        /// <param name="productoExistente">Producto a actualizar</param>
        /// <param name="nuevosDatos">Nuevos datos del producto</param>
        /// <remarks>
        /// ✅ CORRECCIÓN: Mantiene el item original pero actualiza el usuario
        /// </remarks>
        private void ActualizarProductoExistente(OrCtlCargaProductoDto productoExistente, OrCtlCargaProductoDto nuevosDatos)
        {
            // Guardar valor que NO debe cambiar
            var itemOriginal = productoExistente.item;

            // Actualizar TODOS los campos con los nuevos datos
            productoExistente.or_compte = nuevosDatos.or_compte;
            productoExistente.p_desc = nuevosDatos.p_desc;
            productoExistente.p_id_prov = nuevosDatos.p_id_prov;
            productoExistente.p_id_barrado = nuevosDatos.p_id_barrado;
            productoExistente.up_id = nuevosDatos.up_id;
            productoExistente.unidad_pres = nuevosDatos.unidad_pres;
            productoExistente.bulto = nuevosDatos.bulto;
            productoExistente.us = nuevosDatos.us;
            productoExistente.cantidad = nuevosDatos.cantidad;
            productoExistente.vto = nuevosDatos.vto;

            // ✅ CORRECCIÓN: ACTUALIZAR USUARIO TAMBIÉN
            productoExistente.usu_id = UserName;

            // Restaurar valor que debe mantenerse
            productoExistente.item = itemOriginal;

            _logger?.LogInformation("🔄 Producto actualizado: {PId} en posición {Item} por usuario {Usuario}",
                productoExistente.p_id, productoExistente.item, UserName);
        }

        /// <summary>
        /// Busca un producto en la lista por su ID
        /// </summary>
        /// <param name="listaProductos">Lista de productos donde buscar</param>
        /// <param name="pId">ID del producto a buscar</param>
        /// <returns>El producto encontrado o null</returns>
        private OrCtlCargaProductoDto? BuscarProductoEnLista(List<OrCtlCargaProductoDto> listaProductos, string pId)
        {
            if (listaProductos == null || listaProductos.Count == 0 || string.IsNullOrWhiteSpace(pId))
                return null;

            return listaProductos.FirstOrDefault(p =>
                p.p_id.Equals(pId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
