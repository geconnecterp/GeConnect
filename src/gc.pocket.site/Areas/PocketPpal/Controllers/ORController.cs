using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "❌ Error inesperado al intentar obtener las OR");
                return Json(new { success = false, message = "Se produjo un error al intentar obtener las OR."});
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
                TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }

            // ✅ REFACTORIZADO: Usar ORSession
            //la inicializacion con nuevo comprobante
            var session = new ORSessionDto(); 
            session.ORComprobanteActual = or_compte;
            session.UltimaActualizacion = DateTime.Now;
            ORSession = session;

            _logger?.LogInformation("📝 OR Seleccionada: {OrCompte}", or_compte);

            var sigla = "OR";
            string? volver = Url.Action("index", "or", new { area = "PocketPpal" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = session.ORComprobanteActual;

            return View("or_lista");
        }

        ///// <summary>
        ///// ✅ NUEVA ACTION: Obtiene la lista de OR según BOX
        ///// </summary>
        ///// <param name="or_compte">ID del comprobante de orden de reparto</param>
        ///// <param name="adm">ID de la administración</param>
        ///// <param name="usu">ID del usuario</param>
        ///// <returns>Lista de OR filtrada por BOX</returns>
        //[HttpPost]
        //public async Task<JsonResult> ObtenerListaORbyBox(string or_compte, string adm, string usu)
        //{
        //    try
        //    {
        //        // Validación de entrada
        //        if (string.IsNullOrWhiteSpace(or_compte))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro or_compte vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de orden de reparto requerido"
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(adm))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro adm vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de administración requerido"
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(usu))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro usu vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de usuario requerido"
        //            });
        //        }

        //        _logger?.LogInformation("📡 Obteniendo lista OR por BOX - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
        //            or_compte, adm, usu);

        //        // Invocar servicio
        //        var resultado = await _orServicio.ObtenerListaORbyBox(
        //            or_compte,
        //            adm,
        //            usu,
        //            TokenCookie
        //        );

        //        if (resultado == null)
        //        {
        //            _logger?.LogError("❌ Respuesta nula del servicio ObtenerListaORbyBox");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Error al obtener lista de OR por BOX"
        //            });
        //        }

        //        if (!resultado.Ok)
        //        {
        //            _logger?.LogWarning("⚠️ Error al obtener lista OR por BOX: {Mensaje}",
        //                resultado.Mensaje);
        //            return Json(new
        //            {
        //                success = false,
        //                message = resultado.Mensaje ?? "Error al obtener lista de OR por BOX"
        //            });
        //        }

        //        _logger?.LogInformation("✅ Lista OR por BOX obtenida correctamente");

        //        return Json(new
        //        {
        //            success = true,
        //            data = resultado.ListaEntidad
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex,
        //            "❌ Error al obtener lista OR por BOX - OR: {OrCompte}",
        //            or_compte);

        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Error al obtener lista de OR por BOX: {ex.Message}"
        //        });
        //    }
        //}

        ///// <summary>
        ///// ✅ NUEVA ACTION: Obtiene la lista de OR según Rubro
        ///// </summary>
        ///// <param name="or_compte">ID del comprobante de orden de reparto</param>
        ///// <param name="adm">ID de la administración</param>
        ///// <param name="usu">ID del usuario</param>
        ///// <returns>Lista de OR filtrada por Rubro</returns>
        //[HttpPost]
        //public async Task<JsonResult> ObtenerListaORbyRubro(string or_compte, string adm, string usu)
        //{
        //    try
        //    {
        //        // Validación de entrada
        //        if (string.IsNullOrWhiteSpace(or_compte))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro or_compte vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de orden de reparto requerido"
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(adm))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro adm vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de administración requerido"
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(usu))
        //        {
        //            _logger?.LogWarning("⚠️ Parámetro usu vacío");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "ID de usuario requerido"
        //            });
        //        }

        //        _logger?.LogInformation("📡 Obteniendo lista OR por Rubro - OR: {OrCompte}, ADM: {Adm}, USU: {Usu}",
        //            or_compte, adm, usu);

        //        // Invocar servicio
        //        var resultado = await _orServicio.ObtenerListaORbyRubro(
        //            or_compte,
        //            adm,
        //            usu,
        //            TokenCookie
        //        );

        //        if (resultado == null)
        //        {
        //            _logger?.LogError("❌ Respuesta nula del servicio ObtenerListaORbyRubro");
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Error al obtener lista de OR por Rubro"
        //            });
        //        }

        //        if (!resultado.Ok)
        //        {
        //            _logger?.LogWarning("⚠️ Error al obtener lista OR por Rubro: {Mensaje}",
        //                resultado.Mensaje);
        //            return Json(new
        //            {
        //                success = false,
        //                message = resultado.Mensaje ?? "Error al obtener lista de OR por Rubro"
        //            });
        //        }

        //        _logger?.LogInformation("✅ Lista OR por Rubro obtenida correctamente");

        //        return Json(new
        //        {
        //            success = true,
        //            data = resultado.ListaEntidad
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex,
        //            "❌ Error al obtener lista OR por Rubro - OR: {OrCompte}",
        //            or_compte);

        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Error al obtener lista de OR por Rubro: {ex.Message}"
        //        });
        //    }
        //}

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
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                // ✅ REFACTORIZADO: Obtener sesión completa
                var session = ORSession;

                //si los parametros vienen en null y verificamos que ORSession
                //tiene el box_id o el rub_id, se cargan los parametros con el valor
                //resguardado en session.
                if (string.IsNullOrEmpty(box_id) && string.IsNullOrEmpty(rub_id))
                {
                    //analizamos la variable de sesion
                    if (string.IsNullOrEmpty(session.ORBoxSeleccionado) &&
                        string.IsNullOrEmpty(session.ORRubroSeleccionado))
                    {
                        TempData["error"] = "No se encontro ni el box ni el rubro seleccionado";
                        return RedirectToAction("CargaORLista", "or", new { area = "pocketppal", or_compte = session.ORComprobanteActual });
                    }
                    else
                    {
                        //asignamos el valor segun exista en la sesion
                        box_id = session.ORBoxSeleccionado ?? "";
                        rub_id = session.ORRubroSeleccionado ?? "";
                    }

                }


                if (string.IsNullOrEmpty(session.ORComprobanteActual))
                {
                    TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                    return RedirectToAction("index");
                }

                // ✅ Determinar parámetros para el servicio
                string boxIdParam = "%";
                string rubIdParam = "%";

                if (!string.IsNullOrWhiteSpace(box_id))
                {
                    session.ORBoxSeleccionado = box_id;
                    session.ORRubroSeleccionado = null;
                    session.FiltroEsBox = true;
                    boxIdParam = box_id;

                    _logger?.LogInformation("📦 BOX seleccionado: {BoxId}", box_id);
                }
                else if (!string.IsNullOrWhiteSpace(rub_id))
                {
                    session.ORRubroSeleccionado = rub_id;
                    session.ORBoxSeleccionado = null;
                    session.FiltroEsBox = false;
                    rubIdParam = rub_id;

                    _logger?.LogInformation("🏷️ RUBRO seleccionado: {RubId}", rub_id);
                }
                else
                {
                    // Usar valores existentes en sesión
                    if (!string.IsNullOrWhiteSpace(session.ORBoxSeleccionado))
                    {
                        boxIdParam = session.ORBoxSeleccionado;
                        session.FiltroEsBox = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(session.ORRubroSeleccionado))
                    {
                        rubIdParam = session.ORRubroSeleccionado;
                        session.FiltroEsBox = false;
                    }
                }

                // Preparar request
                var request = new ORProdRequestDto
                {
                    or_compte = session.ORComprobanteActual,
                    adm_id = AdministracionId,
                    usu_id = UserName,
                    box_id = boxIdParam,
                    rub_id = rubIdParam
                };

                _logger?.LogInformation(
                    "📡 Obteniendo productos - Compte: {Compte}, Box: {Box}, Rub: {Rub}",
                    request.or_compte, request.box_id, request.rub_id);

                // Invocar servicio
                var resultado = await _orServicio.ObtenerORProductos(request, TokenCookie);

                if (resultado == null || !resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al obtener productos: {Msg}", resultado?.Mensaje);
                    TempData["warn"] = resultado?.Mensaje ?? "No se encontraron productos.";
                    return RedirectToAction("CargaORLista", new { or_compte = session.ORComprobanteActual });
                }

                // ✅ Guardar productos en sesión
                session.ORListaProductosActual = resultado.ListaEntidad ?? new List<ORProductoDto>();
                session.UltimaActualizacion = DateTime.Now;
                ORSession = session;

                _logger?.LogInformation("✅ {Count} productos guardados en sesión", session.ORListaProductosActual.Count);

                // Configurar ViewBag
                var sigla = "OR";
                string? volver = Url.Action("CargaORLista", "or",
                    new { area = "PocketPpal", or_compte = session.ORComprobanteActual });

                var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x =>
                    x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

                if (modulo == null)
                {
                    throw new NegocioException("No se logró encontrar la configuración del Módulo.");
                }

                modulo.VolverUrl = volver ?? "#";
                ViewBag.AppItem = modulo;
                ViewBag.Compte = session.ORComprobanteActual;

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error en ORCargaCarrito");
                TempData["error"] = "Error al cargar el carrito de productos.";

                var session = ORSession;
                return RedirectToAction("CargaORLista", new { or_compte = session.ORComprobanteActual });
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
            try
            {
                _logger?.LogInformation("📋 Ordenando productos - Criterio: {Orden}", orden);

                // ✅ REFACTORIZADO: Obtener productos desde ORSession
                var session = ORSession;
                var productos = session.ORListaProductosActual;

                if (productos == null || !productos.Any())
                {
                    _logger?.LogWarning("⚠️ No hay productos en sesión");

                    var gridVacio = new GridCoreSmart<ORProductoDto>
                    {
                        ListaDatos = new StaticPagedList<ORProductoDto>(new List<ORProductoDto>(), 1, 999, 0),
                        CantidadReg = 999,
                        PaginaActual = 1,
                        CantidadPaginas = 1,
                        Sort = "p_id",
                        SortDir = "ASC"
                    };

                    return PartialView("_gridORListaProducto", gridVacio);
                }

                // Ordenar según criterio
                List<ORProductoDto> productosOrdenados = orden?.ToUpper() switch
                {
                    "B" => productos.OrderBy(x => x.box_id).ThenBy(x => x.p_desc).ToList(),
                    "R" => productos.OrderBy(x => x.rub_id).ThenBy(x => x.p_desc).ToList(),
                    _ => productos.OrderBy(x => x.p_desc).ToList()
                };

                // ✅ Actualizar sesión con lista ordenada
                session.ORListaProductosActual = productosOrdenados;
                session.UltimaActualizacion = DateTime.Now;
                ORSession = session;

                _logger?.LogInformation("✅ {Count} productos ordenados y guardados", productosOrdenados.Count);

                var grid = ObtenerGrillaORListaProductos(productosOrdenados, orden ?? "P");

                return PartialView("_gridORListaProducto", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error al ordenar productos OR");
                TempData["error"] = ex.Message;
                return RedirectToAction("ORCargaCarrito");
            }
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

        [HttpGet]
        public IActionResult ORValidaProducto(string p_id)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(p_id))
            {
                TempData["error"] = "No se recepcionó el ID del Producto.";
                return RedirectToAction("ORCargaCarrito");
            }

            // ✅ REFACTORIZADO: Usar ORSession "7794000006294"
            var session = ORSession;
            var producto = session.ORListaProductosActual?.FirstOrDefault(x => x.p_id == p_id);

            if (producto == null)
            {
                TempData["error"] = "Producto no encontrado en la lista actual.";
                return RedirectToAction("ORCargaCarrito");
            }

            // ✅ Guardar producto seleccionado en sesión
            session.ORProductoSeleccionado = p_id;
            session.UltimaActualizacion = DateTime.Now;
            ORSession = session;

            _logger?.LogInformation("✅ Producto seleccionado: {PId} - {Desc}", p_id, producto.p_desc);


            // Configurar ViewBag
            var sigla = "OR";
            string? volver = Url.Action("ORCargaCarrito", "or",
                new { area = "PocketPpal", or_compte = session.ORComprobanteActual });

            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x =>
                x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo.");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = session.ORComprobanteActual;

            //lo tengo que mandar para hacer la comparativa de si la cantidad 
            //solicitada es correcta o no.
            ViewBag.Producto = producto;

            return View((string.Empty, session.ORComprobanteActual));
        }

        [HttpPost]
        public IActionResult ValidarProductoIngresado(string pId)
        {
            string prod = string.Empty;
            try
            {
                var sesion = ORSession;

                prod = sesion.ORProductoSeleccionado ?? "";


                if (prod != null && prod.Equals(pId))
                {
                    return Json(new { error = false, warn = false, msg = "Producto es Correcto" });
                }
                else
                {
                    throw new NegocioException("El Producto ingresado no corresponde al Producto esperado");
                }

            }
            catch (NegocioException ex)
            {

                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} Producto ingresado:{pId} - Producto Esperado {prod}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} - antes de continuar debera autenticarse nuevamente.");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {

                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} Producto ingresado:{pId} - Producto Esperado {prod}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Valida que el BOX ingresado coincida con el seleccionado en sesión
        /// </summary>
        /// <param name="boxIngresado">Código de BOX escaneado por el usuario</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost]
        public IActionResult ValidarBoxIngresado(string boxIngresado)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(boxIngresado))
                {
                    _logger?.LogWarning("⚠️ Validación BOX: parámetro vacío");
                    return Json(new
                    {
                        success = false,
                        message = "Debe ingresar un código de BOX"
                    });
                }

                boxIngresado = boxIngresado.Trim();

                if (boxIngresado.Length != 11)
                {
                    _logger?.LogWarning("⚠️ Validación BOX: longitud incorrecta ({Length})", boxIngresado.Length);
                    return Json(new
                    {
                        success = false,
                        message = "El código de BOX debe tener 11 caracteres"
                    });
                }

                // ✅ Obtener sesión OR
                var session = ORSession;

                if (string.IsNullOrEmpty(session.ORComprobanteActual))
                {
                    _logger?.LogWarning("⚠️ Validación BOX: sin comprobante en sesión");
                    return Json(new
                    {
                        success = false,
                        message = "No hay una orden de reparto activa en sesión"
                    });
                }

                // ✅ Validar según el tipo de filtro usado
                string boxEnSesion = string.Empty;

                if (session.FiltroEsBox && !string.IsNullOrWhiteSpace(session.ORBoxSeleccionado))
                {
                    boxEnSesion = session.ORBoxSeleccionado;
                }
                else if (!session.FiltroEsBox)
                {
                    // Si se filtró por rubro, obtener el box del producto seleccionado
                    var productoActual = session.ORListaProductosActual?
                        .FirstOrDefault(p => p.p_id == session.ORProductoSeleccionado);

                    if (productoActual != null)
                    {
                        boxEnSesion = productoActual.box_id;
                    }
                }

                if (string.IsNullOrWhiteSpace(boxEnSesion))
                {
                    _logger?.LogWarning("⚠️ Validación BOX: sin BOX seleccionado en sesión");
                    return Json(new
                    {
                        success = false,
                        message = "No hay un BOX seleccionado en la sesión actual"
                    });
                }

                _logger?.LogInformation("🔍 Comparando BOX - Ingresado: {Ingresado}, Sesión: {Sesion}",
                    boxIngresado, boxEnSesion);

                // ✅ Comparación exacta (case-sensitive)
                if (boxIngresado.Equals(boxEnSesion, StringComparison.Ordinal))
                {
                    _logger?.LogInformation("✅ BOX validado correctamente: {BoxId}", boxIngresado);

                    // Actualizar timestamp de sesión
                    session.UltimaActualizacion = DateTime.Now;
                    ORSession = session;

                    return Json(new
                    {
                        success = true,
                        message = "BOX validado correctamente",
                        data = new
                        {
                            boxId = boxIngresado,
                            comprobante = session.ORComprobanteActual
                        }
                    });
                }
                else
                {
                    _logger?.LogWarning("⚠️ BOX no coincide - Esperado: {Esperado}, Ingresado: {Ingresado}",
                        boxEnSesion, boxIngresado);

                    return Json(new
                    {
                        success = false,
                        message = $"El BOX ingresado no coincide. Se esperaba: {boxEnSesion}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error al validar BOX ingresado: {Box}", boxIngresado);

                return Json(new
                {
                    success = false,
                    message = $"Error al validar BOX: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LimpiaProductoCarritoOR(string p_id, string boxId = "")
        {
            try
            {
                var sesion = ORSession;

                var prod = sesion.ORListaProductosActual.FirstOrDefault(x => x.p_id == p_id);

                if (prod == null)
                {
                    return Json(new { error = false, warn = true, msg = $"No se encontró el producto en la lista actual." });
                }

                ORCargaCarritoRequest request = new ORCargaCarritoRequest();
                request.or_compte = prod.ti;
                request.adm_id = AdministracionId;
                request.usu_id = UserName;
                request.box_id = prod.box_id;
                request.desarma_box = true;
                request.p_id = prod.p_id;
                request.unidad_pres = prod.unidad_pres;
                request.bulto = 0;
                request.us = 0;
                request.cantidad = 0;
                request.fv = DateTime.MinValue.ToStringYYYYMMDD();

                RespuestaGenerica<RespuestaDto> respv = await _orServicio.ValidaProductoCarritoOR(request, TokenCookie);
                if (respv.Ok)
                {
                    RespuestaGenerica<RespuestaDto> resp = await _orServicio.ResguardarProductoCarrito(request, TokenCookie);

                    if (resp.Ok)
                    {
                        return Json(new { error = false, warn = false, msg = $"Producto {ProductoBase.P_desc} fue Limpiado exitosamente" });
                    }
                    else { return Json(new { error = false, warn = true, msg = resp.Mensaje }); }
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = respv.Mensaje });
                }
            }
            catch (NegocioException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name}params: {p_id}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} params: {p_id} ");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} params: {p_id} ");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResguardarProductoCarritoOR(string p_id, int up, int bulto, decimal unid, decimal cantidad, DateTime? fv)//, bool desarma = true)
        {
            try
            {
                var sesion = ORSession;

                var prod = sesion.ORListaProductosActual.FirstOrDefault(x => x.p_id == p_id);

                if (prod == null)
                {
                    return Json(new { error = false, warn = true, msg = $"No se encontró el producto en la lista actual." });
                }

                if (cantidad < 1)// && desarma)
                {
                    return Json(new { error = false, warn = true, msg = $"La cantidades de los productos a cargar siempre tienen que ser positivas, mayores a 0 (cero)." });
                }
                if (!CantidadCompatibleConUnidadProducto(ProductoBase.up_id, unid) ||
                    !CantidadCompatibleConUnidadProducto(ProductoBase.up_id, cantidad))
                {
                    return Json(new { error = false, warn = true, msg = MensajeCantidadIncompatible(ProductoBase.up_id) });
                }
                var cantidadEsperada = ProductoBase.up_id.Equals("07") ? (up * bulto) + unid : unid;
                if (cantidad != cantidadEsperada)
                {
                    return Json(new { error = false, warn = true, msg = "La cantidad informada no coincide con los bultos y unidades ingresados. Verifique, por favor." });
                }
                if (prod.pedido < cantidad && ProductoBase.up_id.Equals("07"))// && (!TIActual.SinAU || !desarma)) //verificamos las cantidades siempre y cuando haya una autorización o en el caso de transferencia de box completo con desarma = false
                {
                    return Json(new { error = false, warn = true, msg = $"No se puede cargar más unidades o cantidades ({cantidad}) que las pedidas ({prod.pedido})" });
                }
                //DEBO VALIAR SI ES PESABLE UP_ID != 07 QUE LA UP==1
                if (!ProductoBase.up_id.Equals("07") && up != 1)// && desarma)
                {
                    return Json(new { error = false, warn = true, msg = $"EL PRODUCTO NO ES POR UNIDADES. LA UNIDAD DE PRESENTACIÓN TIENE QUE SER IGUAL A 1 SIEMPRE." });
                }
                ////VALIDAR LA FECHA FV CON LA FECHA DE CONTROL (SOLO PARA TRANSFERENCIA DE SUCURSALES)
                //var fechaControl = ProductoBase.p_con_vto_ctl;

                //if (ProductoBase.P_con_vto.Equals("S") && (fv == null || fechaControl > fv.Value) && prod.TipoTI.Equals("S"))
                //{
                //    return Json(new { error = false, warn = true, msg = $"LA FECHA DE CONTROL DEL PRODUCTO {ProductoBase.P_desc} NO ES VALIDA." });
                //}

                ORCargaCarritoRequest request = new ORCargaCarritoRequest();

                request.or_compte = prod.ti;
                request.adm_id = AdministracionId;
                request.usu_id = UserName;
                request.box_id = prod.box_id;
                request.desarma_box = true;
                request.p_id = prod.p_id;
                request.unidad_pres = up;
                request.bulto = bulto;
                request.us = unid;
                request.cantidad = cantidad;

                if (fv.HasValue)
                {
                    request.fv = fv.Value.ToStringYYYYMMDD();   ///debo traer fecha de vencimiento del producto a mostrar
                }
                else
                {
                    request.fv = "19700101";
                }

                RespuestaGenerica<RespuestaDto> respv = await _orServicio.ValidaProductoCarritoOR(request, TokenCookie);
                if (respv.Ok)
                {
                    RespuestaGenerica<RespuestaDto> resp = await _orServicio.ResguardarProductoCarrito(request, TokenCookie);

                    if (resp.Ok)
                    {
                        return Json(new { error = false, warn = false, msg = $"Producto {ProductoBase.P_desc} fue cargado exitosamente" });
                    }
                    else { return Json(new { error = false, warn = true, msg = resp.Mensaje }); }
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = respv.Mensaje });
                }

            }
            catch (NegocioException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name}params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name} params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

    }
}
