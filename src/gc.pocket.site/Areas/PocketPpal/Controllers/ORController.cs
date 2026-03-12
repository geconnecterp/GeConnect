using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ORController : PocketControllerBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly IORServicio _orServicio;
        private readonly AppSettings _appSettings;

        public ORController(IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ILogger<TrIntController> logger,
            IOptions<MenuSettings> options1,
            IORServicio oRServicio,
            IOptions<AppSettings> options2) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _orServicio = oRServicio;
            _appSettings = options2.Value;
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
            var sigla = "OR";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerOrdenesReparto()
        {
            try
            {
                var resp = await _orServicio.ObtenerOrdenesReparto(new ORRequestDto
                {
                    HasFecha = false,
                    Desde = new DateTime(1900, 01, 01),
                    Hasta = new DateTime(2900, 01, 01),
                    HasEstado = true,
                    Ore_list = "O,",
                    HasRepartidor = false,
                    RP_List = string.Empty,
                    HasId = false,
                    OR_Compte = string.Empty,
                    Registros = _appSettings.NroRegistrosPagina,
                    Pagina = 1
                }, TokenCookie);

                if (resp.Ok)
                {
                    return Json(new { success = true, data = resp.ListaEntidad });
                }
                else
                {
                    return Json(new { success = false, message = resp.Mensaje });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Valida si el usuario puede procesar la orden de reparto
        /// </summary>
        /// <param name="orCompte">ID del comprobante de orden de reparto</param>
        /// <param name="usuId">ID del usuario a validar</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost]
        public async Task<JsonResult> ValidarUsuario(string orCompte, string usuId)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(orCompte))
                {
                    _logger?.LogWarning("⚠️ Validación fallida: ID de orden vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de orden de reparto requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(usuId))
                {
                    _logger?.LogWarning("⚠️ Validación fallida: ID de usuario vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de usuario requerido"
                    });
                }

                _logger?.LogInformation("📡 Validando usuario {UsuId} para orden {OrCompte}",
                    usuId, orCompte);

                // Invocar servicio de validación
                var resultado = await _orServicio.ValidarUsuario(
                    orCompte,
                    usuId,
                    TokenCookie
                );

                if (resultado == null)
                {
                    _logger?.LogError("❌ Respuesta nula del servicio de validación");
                    return Json(new
                    {
                        success = false,
                        message = "Error al validar usuario"
                    });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Validación de usuario fallida: {Mensaje}",
                        resultado.Mensaje);
                    return Json(new
                    {
                        success = false,
                        message = resultado.Mensaje ?? "Validación de usuario fallida"
                    });
                }

                _logger?.LogInformation("✅ Usuario validado correctamente para orden {OrCompte}",
                    orCompte);

                return Json(new
                {
                    success = true,
                    message = "Usuario validado correctamente",
                    data = resultado.Entidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "❌ Error al validar usuario {UsuId} para orden {OrCompte}",
                    usuId, orCompte);

                return Json(new
                {
                    success = false,
                    message = $"Error al validar usuario: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public IActionResult CargaORLista(string or_compte)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(or_compte))
            {
                TempData["error"] = "No se recepciono el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }
            ORComprobanteActual = or_compte;

            var sigla = "OR";
            string? volver = Url.Action("index", "or", new { area = "PocketPpal" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = ORComprobanteActual;

            return View("or_lista");
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Obtiene la lista de OR según BOX
        /// </summary>
        /// <param name="or_compte">ID del comprobante de orden de reparto</param>
        /// <param name="adm">ID de la administración</param>
        /// <param name="usu">ID del usuario</param>
        /// <returns>Lista de OR filtrada por BOX</returns>
        [HttpPost]
        public async Task<JsonResult> ObtenerListaORbyBox(string or_compte, string adm, string usu)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(or_compte))
                {
                    _logger?.LogWarning("⚠️ Parámetro or_compte vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de orden de reparto requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(adm))
                {
                    _logger?.LogWarning("⚠️ Parámetro adm vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de administración requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(usu))
                {
                    _logger?.LogWarning("⚠️ Parámetro usu vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de usuario requerido"
                    });
                }

                _logger?.LogInformation("📡 Obteniendo lista OR por BOX - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
                    or_compte, adm, usu);

                // Invocar servicio
                var resultado = await _orServicio.ObtenerListaORbyBox(
                    or_compte,
                    adm,
                    usu,
                    TokenCookie
                );

                if (resultado == null)
                {
                    _logger?.LogError("❌ Respuesta nula del servicio ObtenerListaORbyBox");
                    return Json(new
                    {
                        success = false,
                        message = "Error al obtener lista de OR por BOX"
                    });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al obtener lista OR por BOX: {Mensaje}",
                        resultado.Mensaje);
                    return Json(new
                    {
                        success = false,
                        message = resultado.Mensaje ?? "Error al obtener lista de OR por BOX"
                    });
                }

                _logger?.LogInformation("✅ Lista OR por BOX obtenida correctamente");

                return Json(new
                {
                    success = true,
                    data = resultado.ListaEntidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "❌ Error al obtener lista OR por BOX - OR: {OrCompte}",
                    or_compte);

                return Json(new
                {
                    success = false,
                    message = $"Error al obtener lista de OR por BOX: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Obtiene la lista de OR según Rubro
        /// </summary>
        /// <param name="or_compte">ID del comprobante de orden de reparto</param>
        /// <param name="adm">ID de la administración</param>
        /// <param name="usu">ID del usuario</param>
        /// <returns>Lista de OR filtrada por Rubro</returns>
        [HttpPost]
        public async Task<JsonResult> ObtenerListaORbyRubro(string or_compte, string adm, string usu)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(or_compte))
                {
                    _logger?.LogWarning("⚠️ Parámetro or_compte vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de orden de reparto requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(adm))
                {
                    _logger?.LogWarning("⚠️ Parámetro adm vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de administración requerido"
                    });
                }

                if (string.IsNullOrWhiteSpace(usu))
                {
                    _logger?.LogWarning("⚠️ Parámetro usu vacío");
                    return Json(new
                    {
                        success = false,
                        message = "ID de usuario requerido"
                    });
                }

                _logger?.LogInformation("📡 Obteniendo lista OR por Rubro - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
                    or_compte, adm, usu);

                // Invocar servicio
                var resultado = await _orServicio.ObtenerListaORbyRubro(
                    or_compte,
                    adm,
                    usu,
                    TokenCookie
                );

                if (resultado == null)
                {
                    _logger?.LogError("❌ Respuesta nula del servicio ObtenerListaORbyRubro");
                    return Json(new
                    {
                        success = false,
                        message = "Error al obtener lista de OR por Rubro"
                    });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al obtener lista OR por Rubro: {Mensaje}",
                        resultado.Mensaje);
                    return Json(new
                    {
                        success = false,
                        message = resultado.Mensaje ?? "Error al obtener lista de OR por Rubro"
                    });
                }

                _logger?.LogInformation("✅ Lista OR por Rubro obtenida correctamente");

                return Json(new
                {
                    success = true,
                    data = resultado.ListaEntidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "❌ Error al obtener lista OR por Rubro - OR: {OrCompte}",
                    or_compte);

                return Json(new
                {
                    success = false,
                    message = $"Error al obtener lista de OR por Rubro: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Renderiza la vista parcial con lista de OR por BOX
        /// </summary>
        /// <param name="or_compte">ID del comprobante de orden de reparto</param>
        /// <param name="adm">ID de la administración</param>
        /// <param name="usu">ID del usuario</param>
        /// <returns>Vista parcial con grid de datos</returns>
        [HttpPost]
        public async Task<IActionResult> PresentarListaORbyBox(string or_compte, string adm, string usu)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<ORListaDto> grillaDatos;

            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(or_compte))
                {
                    throw new NegocioException("ID de orden de reparto requerido");
                }

                if (string.IsNullOrWhiteSpace(adm))
                {
                    throw new NegocioException("ID de administración requerido");
                }

                if (string.IsNullOrWhiteSpace(usu))
                {
                    throw new NegocioException("ID de usuario requerido");
                }

                _logger?.LogInformation("📊 Renderizando vista OR por BOX - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
                    or_compte, adm, usu);

                // Invocar servicio
                var resultado = await _orServicio.ObtenerListaORbyBox(
                    or_compte,
                    adm,
                    usu,
                    TokenCookie
                );

                if (resultado == null)
                {
                    throw new NegocioException("Error al obtener lista de OR por BOX");
                }

                if (!resultado.Ok)
                {
                    throw new NegocioException(resultado.Mensaje ?? "Error al obtener lista de OR por BOX");
                }

                // Generar grilla con helper
                grillaDatos = GenerarGrilla(resultado.ListaEntidad, "box_id");

                _logger?.LogInformation("✅ Vista OR por BOX renderizada correctamente - {Count} registros",
                    resultado.ListaEntidad?.Count ?? 0);
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "⚠️ Error de negocio al renderizar lista OR por BOX");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "🔒 Usuario no autorizado al renderizar lista OR por BOX");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al renderizar lista OR por BOX");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }

            return PartialView("_OR_Lista_Box", grillaDatos);
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Renderiza la vista parcial con lista de OR por Rubro
        /// </summary>
        /// <param name="or_compte">ID del comprobante de orden de reparto</param>
        /// <param name="adm">ID de la administración</param>
        /// <param name="usu">ID del usuario</param>
        /// <returns>Vista parcial con grid de datos</returns>
        [HttpPost]
        public async Task<IActionResult> PresentarListaORbyRubro(string or_compte, string adm, string usu)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<ORListaDto> grillaDatos;

            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(or_compte))
                {
                    throw new NegocioException("ID de orden de reparto requerido");
                }

                if (string.IsNullOrWhiteSpace(adm))
                {
                    throw new NegocioException("ID de administración requerido");
                }

                if (string.IsNullOrWhiteSpace(usu))
                {
                    throw new NegocioException("ID de usuario requerido");
                }

                _logger?.LogInformation("📊 Renderizando vista OR por Rubro - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
                    or_compte, adm, usu);

                // Invocar servicio
                var resultado = await _orServicio.ObtenerListaORbyRubro(
                    or_compte,
                    adm,
                    usu,
                    TokenCookie
                );

                if (resultado == null)
                {
                    throw new NegocioException("Error al obtener lista de OR por Rubro");
                }

                if (!resultado.Ok)
                {
                    throw new NegocioException(resultado.Mensaje ?? "Error al obtener lista de OR por Rubro");
                }

                // Generar grilla con helper
                grillaDatos = GenerarGrilla(resultado.ListaEntidad, "rub_id");

                _logger?.LogInformation("✅ Vista OR por Rubro renderizada correctamente - {Count} registros",
                    resultado.ListaEntidad?.Count ?? 0);
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "⚠️ Error de negocio al renderizar lista OR por Rubro");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "🔒 Usuario no autorizado al renderizar lista OR por Rubro");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al renderizar lista OR por Rubro");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }

            return PartialView("_OR_Lista_Rub", grillaDatos);
        }


        /// <summary>
        /// ✅ MODIFICADO: Action para cargar el carrito de productos de OR
        /// Recibe box_id o rub_id según la selección del usuario
        /// Obtiene los productos del servicio y los guarda en sesión
        /// </summary>
        /// <param name="box_id">ID del BOX seleccionado (opcional)</param>
        /// <param name="rub_id">ID del RUBRO seleccionado (opcional)</param>
        /// <returns>Vista ORCargaCarrito</returns>
        [HttpGet]
        public async Task<IActionResult> ORCargaCarrito(string box_id, string rub_id)
        {
            try
            {
                // Validar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                // Validar que el comprobante seleccionado originalmente exista
                if (string.IsNullOrEmpty(ORComprobanteActual))
                {
                    TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                    return RedirectToAction("index");
                }

                // ✅ Determinar parámetros para el servicio
                string boxIdParam = "%";  // Valor por defecto (todos)
                string rubIdParam = "%";  // Valor por defecto (todos)

                // ✅ Guardar BOX o RUBRO en sesión según lo recibido
                if (!string.IsNullOrWhiteSpace(box_id))
                {
                    ORBoxSeleccionado = box_id;
                    ORRubroSeleccionado = string.Empty; // Limpiar rubro si viene BOX
                    boxIdParam = box_id;
                    
                    _logger?.LogInformation("📦 BOX seleccionado guardado en sesión: {BoxId}", box_id);
                }
                else if (!string.IsNullOrWhiteSpace(rub_id))
                {
                    ORRubroSeleccionado = rub_id;
                    ORBoxSeleccionado = string.Empty; // Limpiar box si viene RUBRO
                    rubIdParam = rub_id;
                    
                    _logger?.LogInformation("🏷️ RUBRO seleccionado guardado en sesión: {RubId}", rub_id);
                }
                else
                {
                    // ✅ Si no viene ninguno, usar los valores de sesión existentes
                    _logger?.LogWarning("⚠️ No se recibió BOX ni RUBRO. Usando valores de sesión si existen.");
                    
                    if (!string.IsNullOrWhiteSpace(ORBoxSeleccionado))
                    {
                        boxIdParam = ORBoxSeleccionado;
                        _logger?.LogInformation("📦 Usando BOX de sesión: {BoxId}", ORBoxSeleccionado);
                    }
                    else if (!string.IsNullOrWhiteSpace(ORRubroSeleccionado))
                    {
                        rubIdParam = ORRubroSeleccionado;
                        _logger?.LogInformation("🏷️ Usando RUBRO de sesión: {RubId}", ORRubroSeleccionado);
                    }
                }

                // ✅ Preparar request para el servicio
                var request = new ORProdRequestDto
                {
                    or_compte = ORComprobanteActual,
                    adm_id = AdministracionId,
                    usu_id = UserName,
                    box_id = boxIdParam,
                    rub_id = rubIdParam
                };

                _logger?.LogInformation(
                    "📡 Obteniendo productos OR - Compte: {OrCompte}, Box: {BoxId}, Rub: {RubId}",
                    request.or_compte, request.box_id, request.rub_id);

                // ✅ Invocar servicio para obtener productos
                var resultado = await _orServicio.ObtenerORProductos(request, TokenCookie);

                if (resultado == null)
                {
                    _logger?.LogError("❌ Respuesta nula del servicio ObtenerORProductos");
                    TempData["error"] = "Error al obtener la lista de productos de la OR.";
                    return RedirectToAction("CargaORLista", new { or_compte = ORComprobanteActual });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al obtener productos OR: {Mensaje}", resultado.Mensaje);
                    TempData["warn"] = resultado.Mensaje ?? "No se encontraron productos para los criterios seleccionados.";
                    return RedirectToAction("CargaORLista", new { or_compte = ORComprobanteActual });
                }

                // ✅ Guardar lista de productos en sesión
                ORListaProductosActual = resultado.ListaEntidad ?? new List<ORProductoDto>();

                _logger?.LogInformation(
                    "✅ Productos OR obtenidos y guardados en sesión - Total: {Count} productos",
                    ORListaProductosActual.Count);

                // ✅ Configurar ViewBag para la vista
                string or = ORComprobanteActual;
                var sigla = "OR";
                string? volver = Url.Action("CargaORLista", "or", new { area = "PocketPpal", or_compte = or });
                var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
                
                if (modulo == null)
                {
                    throw new NegocioException("No se logró encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
                }
                
                modulo.VolverUrl = volver ?? "#";
                ViewBag.AppItem = modulo;
                ViewBag.Compte = ORComprobanteActual;

                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "⚠️ Error de negocio en ORCargaCarrito");
                TempData["warn"] = ex.Message;
                return RedirectToAction("CargaORLista", new { or_compte = ORComprobanteActual });
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "🔒 Usuario no autorizado en ORCargaCarrito");
                TempData["warn"] = ex.Message;
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado en ORCargaCarrito");
                TempData["error"] = "Error al cargar el carrito de productos. Por favor, intente nuevamente.";
                return RedirectToAction("CargaORLista", new { or_compte = ORComprobanteActual });
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Busca y ordena productos de OR según criterio
        /// Similar a BuscaTIListaProductos de TrIntController
        /// </summary>
        /// <param name="orden">Criterio de ordenamiento: B (BOX), R (RUBRO), P (PRODUCTO)</param>
        /// <returns>Vista parcial con grid de productos</returns>
        [HttpPost]
        public IActionResult BuscaORListaProductos(string orden)
        {
            GridCoreSmart<ORProductoDto> grid;

            try
            {
                _logger?.LogInformation("📋 Buscando lista de productos OR - Orden: {Orden}", orden);

                // Obtener productos de sesión
                var productos = ORListaProductosActual;

                if (productos == null || !productos.Any())
                {
                    _logger?.LogWarning("⚠️ No hay productos en sesión");

                    // Retornar grid vacío
                    grid = new GridCoreSmart<ORProductoDto>
                    {
                        ListaDatos = new StaticPagedList<ORProductoDto>(new List<ORProductoDto>(), 1, 999, 0),
                        CantidadReg = 999,
                        PaginaActual = 1,
                        CantidadPaginas = 1,
                        Sort = "p_id",
                        SortDir = "ASC"
                    };

                    return PartialView("_gridORListaProducto", grid);
                }

                // ✅ Ordenar según criterio
                List<ORProductoDto> productosOrdenados;

                switch (orden?.ToUpper())
                {
                    case "B": // Ordenar por BOX
                        productosOrdenados = productos.OrderBy(x => x.box_id)
                                                     .ThenBy(x => x.p_desc)
                                                     .ToList();
                        _logger?.LogInformation("📦 Productos ordenados por BOX");
                        break;

                    case "R": // Ordenar por RUBRO
                        productosOrdenados = productos.OrderBy(x => x.rub_id)
                                                     .ThenBy(x => x.p_desc)
                                                     .ToList();
                        _logger?.LogInformation("🏷️ Productos ordenados por RUBRO");
                        break;

                    case "P": // Ordenar por PRODUCTO
                    default:
                        productosOrdenados = productos.OrderBy(x => x.p_desc)
                                                     .ToList();
                        _logger?.LogInformation("📝 Productos ordenados por PRODUCTO");
                        break;
                }

                // ✅ Actualizar variable de sesión con lista ordenada
                ORListaProductosActual = productosOrdenados;

                // ✅ Generar grid
                grid = ObtenerGrillaORListaProductos(productosOrdenados, orden ?? "P");

                _logger?.LogInformation("✅ Grid generado - {Count} productos", productosOrdenados.Count);
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "⚠️ Error de negocio al buscar productos OR");
                TempData["warn"] = ex.Message;
                return RedirectToAction("ORCargaCarrito");
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "🔒 Usuario no autorizado");
                TempData["warn"] = ex.Message;
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al buscar productos OR");
                TempData["error"] = ex.Message;
                return RedirectToAction("ORCargaCarrito");
            }

            return PartialView("_gridORListaProducto", grid);
        }

        /// <summary>
        /// ✅ Helper: Genera GridCoreSmart para lista de productos OR
        /// </summary>
        /// <param name="productos">Lista de productos</param>
        /// <param name="sortColumn">Columna de ordenamiento</param>
        /// <returns>Grid configurado</returns>
        private GridCoreSmart<ORProductoDto> ObtenerGrillaORListaProductos(List<ORProductoDto> productos, string sortColumn)
        {
            if (productos == null)
            {
                productos = new List<ORProductoDto>();
            }

            var lista = new StaticPagedList<ORProductoDto>(productos, 1, 999, productos.Count);

            // Determinar columna de ordenamiento para metadata
            string sortField = sortColumn switch
            {
                "B" => "Box_id",
                "R" => "Rub_id",
                "P" => "P_desc",
                _ => "P_desc"
            };

            return new GridCoreSmart<ORProductoDto>
            {
                ListaDatos = lista,
                CantidadReg = 999,
                PaginaActual = 1,
                CantidadPaginas = 1,
                Sort = sortField,
                SortDir = "ASC"
            };
        }
    }
}
