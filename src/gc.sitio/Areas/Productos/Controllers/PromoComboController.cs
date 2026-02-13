using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class PromoComboController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.PROMO_COMBO.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;
        private readonly IComboServicio _comboServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public PromoComboController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
           ILogger<OfertasController> logger, IComboServicio comboSv,
           ICuentaServicio cuenta, IRubroServicio rubro, IOptions<DocsManager> docsManager,
           IDocManagerServicio docManagerServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _comboServicio = comboSv;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
        }

        public IActionResult Index()
        {
            string msg = "Error de negocio al cargar la vista de PROMOS Y COMBOS";
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Inicializar lista al ingresar el modulo
                ProductosSeleccionadosV02 = [];

                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                //DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);

                //_logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                ////en este mismo acto se cargan los posibles documentos
                ////que se pueden imprimir, exportar, enviar por email o whatsapp
                //ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                CargarDatosIniciales(true, _cuentaServicio, _rubroServicio,_comboServicio);

                ViewBag.Tipo = ComboTipoCombo();
                ViewBag.cmb_tipo = ComboTipoCombo();
                ViewBag.Estado = ComboEstadoCombo();
                ViewBag.cmb_estado = ComboEstadoCombo();


                ViewData["Titulo"] = "PROMOS y COMBOS";

                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = "Hubo un problema al cargar la vista de PROMOS y COMBOS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PresentarPromosYCombos(QueryFilters filtros)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                int cantTotalReg = 0;

                filtros.Tipo = string.IsNullOrEmpty(filtros.Tipo) ? "%" : filtros.Tipo;
                filtros.Estado = string.IsNullOrEmpty(filtros.Estado) ? "%" : filtros.Estado;
                filtros.Registros = _configuracion.NroRegistrosPagina;

                // Llamar al servicio para buscar combos
                RespuestaGenerica<ComboListaDto> respuesta = await _comboServicio.BuscarCombos(filtros, TokenCookie);
                
                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                {
                    TempData["error"] = respuesta.Mensaje ?? "Error al obtener promociones y combos";
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje));
                }
                
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridMensaje", CrearRespuestaWarning("No se encontraron promociones ni combos"));
                }
                MetadataGeneral = respuesta.Meta;
                // Obtener lista de combos
                var combos = respuesta.ListaEntidad;
                if (combos.Count > 0)
                {
                    cantTotalReg = combos[0].Total_Registros;
                }
                int registrosPorPagina = _configuracion.NroRegistrosPagina;

                int page = filtros.Pagina ?? 1;
                // Crear lista paginada
                var pagedList = new StaticPagedList<ComboListaDto>(
                    combos,
                    page,
                    registrosPorPagina,
                    cantTotalReg
                );
          
                // Configurar el GridCoreSmart
                var grid = new GridCoreSmart<ComboListaDto>
                {
                    ListaDatos = pagedList, //lista de combos
                    CantidadReg = combos.Count, //cantidad actual de registros
                    PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
                    UltimoRegistro = Math.Min(page * registrosPorPagina, combos.Count), //define cual es el ultimo registro
                    RegistroFinal = combos.Count, //indica cual es el ultimo registro
                    CantidadPaginas = (int)Math.Ceiling((double)combos.Count / registrosPorPagina),//calcula la cantidad de paginas
                    PaginaActual = page,//especifica que pagina es la actual
                    Sort = filtros.Sort ?? "descripcion", 
                    SortDir = filtros.SortDir ?? "ASC",
                    DatoAux01 = $"Promociones y combos cargados: {DateTime.Now:HH:mm:ss}"
                };

                // Devolver vista parcial with el grid
                return PartialView("_gridPromoCombo", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar promociones y combos");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar promociones y combos");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar promociones y combos"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCanalesCombo(string id)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar ID
                if (string.IsNullOrEmpty(id))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("El identificador del combo no es válido"));

                // Llamar al servicio para obtener canales del combo
                var respuesta = await _comboServicio.ObtenerCanalesDeCombo(id, TokenCookie);
                
                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener canales"));
                }
                
                // Verificar si hay canales
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridComboCanales", new GridCoreSmart<ComboCanalDto>());
                }
                
                // Crear grid con los canales
                var grid = new GridCoreSmart<ComboCanalDto>
                {
                    ListaDatos = new StaticPagedList<ComboCanalDto>(respuesta.ListaEntidad, 1, respuesta.ListaEntidad.Count, respuesta.ListaEntidad.Count),
                    CantidadReg = respuesta.ListaEntidad.Count,
                    PrimerRegistro = 1,
                    UltimoRegistro = respuesta.ListaEntidad.Count,
                    RegistroFinal = respuesta.ListaEntidad.Count,
                    CantidadPaginas = 1,
                    PaginaActual = 1,
                    DatoAux01 = $"Canales del combo {id} | {DateTime.Now:HH:mm:ss}"
                };
                
                return PartialView("_gridComboCanales", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al obtener canales del combo");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al obtener canales del combo");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener canales del combo"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerComboPorId(string id)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "No autorizado" });

                // Validar ID
                if (string.IsNullOrEmpty(id))
                    return Json(new { ok = false, mensaje = "ID no válido" });

                // Llamar al servicio para obtener datos del combo
                var respuesta = await _comboServicio.ObtenerComboPorId(id, TokenCookie);
                
                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                    return Json(new { ok = false, mensaje = respuesta.Mensaje ?? "Error al obterner datos del combo" });
                
                // Verificar si se encontró el combo
                if (respuesta.Entidad == null)
                    return Json(new { ok = false, mensaje = "No se encontró el combo especificado" });
                
                // Devolver resultado exitoso con los datos del combo
                return Json(new { 
                    ok = true, 
                    entidad = respuesta.Entidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener datos del combo");
                return Json(new { ok = false, mensaje = "Error interno al obtener datos del combo" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDeCombo(string id)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar ID
                if (string.IsNullOrEmpty(id))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("El identificador del combo no es válido"));

                // Llamar al servicio para obtener productos del combo
                var respuesta = await _comboServicio.ObtenerProductosDeCombo(id, TokenCookie);
                
                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener productos"));
                }
                
                // Verificar si hay productos
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridProductos", new GridCoreSmart<ComboProductoDto>());
                }
                
                // Crear grid con los productos
                var grid = new GridCoreSmart<ComboProductoDto>
                {
                    ListaDatos = new StaticPagedList<ComboProductoDto>(respuesta.ListaEntidad, 1, respuesta.ListaEntidad.Count, respuesta.ListaEntidad.Count),
                    CantidadReg = respuesta.ListaEntidad.Count,
                    PrimerRegistro = 1,
                    UltimoRegistro = respuesta.ListaEntidad.Count,
                    RegistroFinal = respuesta.ListaEntidad.Count,
                    CantidadPaginas = 1,
                    PaginaActual = 1,
                    DatoAux01 = $"Productos del combo {id} | {DateTime.Now:HH:mm:ss}"
                };
                
                return PartialView("_gridProductos", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al obtener productos del combo");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al obtener productos del combo");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener productos del combo"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosSustitutos(string comboId, string productoId)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar parámetros
                if (string.IsNullOrEmpty(comboId))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("El identificador del combo no es válido"));
                    
                if (string.IsNullOrEmpty(productoId))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("El identificador del producto no es válido"));

                // Llamar al servicio para obtener sustitutos
                var respuesta = await _comboServicio.ObtenerProductosSustitutosDeCombo(comboId, productoId, TokenCookie);
                
                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener productos sustitutos"));
                }
                
                // Verificar si hay sustitutos - No es error si no hay, simplemente mostramos grid vacío
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridSustitutos", new GridCoreSmart<ComboSustitutoDto>());
                }
                
                // Crear grid con los sustitutos
                var grid = new GridCoreSmart<ComboSustitutoDto>
                {
                    ListaDatos = new StaticPagedList<ComboSustitutoDto>(respuesta.ListaEntidad, 1, respuesta.ListaEntidad.Count, respuesta.ListaEntidad.Count),
                    CantidadReg = respuesta.ListaEntidad.Count,
                    PrimerRegistro = 1,
                    UltimoRegistro = respuesta.ListaEntidad.Count,
                    RegistroFinal = respuesta.ListaEntidad.Count,
                    CantidadPaginas = 1,
                    PaginaActual = 1,
                    DatoAux01 = $"Sustitutos del producto {productoId} en combo {comboId} | {DateTime.Now:HH:mm:ss}"
                };
                
                return PartialView("_gridSustitutos", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al obtener productos sustitutos");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al obtener productos sustitutos");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener productos sustitutos"));
            }
        }

        /// <summary>
        /// Guarda en sesión las relaciones entre un producto y sus sustitutos
        /// </summary>
        /// <param name="request">Objeto que contiene el ID del producto y sus sustitutos</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost]
        public JsonResult ResguardarRelacionProductoSustituto([FromBody] SustitutosRelacionDto request)
        {
            try
            {
                // Validar parámetros de entrada
                if (request == null || string.IsNullOrEmpty(request.p_id))
                {
                    return Json(new { ok = false, mensaje = "El ID del producto es obligatorio" });
                }

                if (request.sus == null || request.sus.Count == 0)
                {
                    return Json(new { ok = false, mensaje = "Debe especificar al menos un sustituto" });
                }

                // Filtrar IDs inválidos y eliminar duplicados
                var sustitutosValidos = request.sus
                    .Where(s => !string.IsNullOrEmpty(s.p_id_sustituto)) // Validamos el campo correcto
                    .GroupBy(s => s.p_id_sustituto)
                    .Select(g => g.First())
                    .ToList();

                if (!sustitutosValidos.Any())
                {
                    return Json(new { ok = false, mensaje = "No hay sustitutos válidos para agregar" });
                }

                // Obtener la lista actual de relaciones producto-sustituto de la sesión               
                var listaRelaciones = ProductosSustitutos ?? new List<ComboSustitutoDto>();

                // Eliminar relaciones existentes para este producto
                listaRelaciones.RemoveAll(r => r.p_id == request.p_id);

                // Agregar las nuevas relaciones
                foreach (var s in sustitutosValidos)
                {
                    listaRelaciones.Add(new ComboSustitutoDto
                    {
                        p_id = request.p_id,
                        p_id_sustituto = s.p_id_sustituto,
                        p_desc = s.p_desc,
                        p_pcosto = s.p_pcosto,
                        activo = s.activo  // Usar valor proporcionado o "A" por defecto
                    });
                }

                // Guardar la lista actualizada en sesión
                ProductosSustitutos = listaRelaciones;

                // Devolver respuesta exitosa
                return Json(new
                {
                    ok = true,
                    mensaje = $"Se guardaron {sustitutosValidos.Count} sustitutos para el producto {request.p_id}",
                    cantidadSustitutos = sustitutosValidos.Count
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al guardar relaciones de productos sustitutos");
                return Json(new { ok = false, mensaje = "Error al procesar la solicitud" });
            }
        }

        /// <summary>
        /// Tiene como misión devolver los productos sustitulos resguardados temporalmente
        /// en la variable "ProductosSustitutos", de un producto solamente durante el proceso 
        /// de ALTA de un nuevo "combo".
        /// </summary>
        /// <param name="p_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RetornarProductosSustitutos(string p_id)
        {
            try
            {
                // Validar que el ID del producto no sea nulo o vacío
                if (string.IsNullOrEmpty(p_id))
                {
                    return Json(new { ok = false, mensaje = "El ID del producto es requerido" });
                }

                // Obtener los productos sustitutos para el producto específico
                var sustitutos = ProductosSustitutos?
                    .Where(s => s.p_id == p_id)
                    .ToList() ?? new List<ComboSustitutoDto>();

                // Devolver el resultado como JSON
                return Json(new { 
                    ok = true, 
                    sustitutos = sustitutos,
                    cantidad = sustitutos.Count,
                    mensaje = sustitutos.Any() 
                        ? $"Se encontraron {sustitutos.Count} sustitutos para el producto {p_id}" 
                        : $"No se encontraron sustitutos para el producto {p_id}"
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al recuperar los productos sustitutos");
                return Json(new { ok = false, mensaje = "Error al procesar la solicitud de productos sustitutos" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCombosRepo([FromBody] ComboReqDto req)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (req == null)
                {
                    throw new NegocioException("No se recepcionaron los parametros de la consulta.");
                }

                // Validar parámetros
                if (string.IsNullOrEmpty(req.adm_id))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("No se reconoce la Administración de la solicitud."));

                if (string.IsNullOrEmpty(req.lp_id))
                    return PartialView("_gridMensaje", CrearRespuestaWarning("No se identifica la lista de precios"));

                // Llamar al servicio para obtener sustitutos
                var respuesta = await _comboServicio.ObtenerCombosRepo(req, TokenCookie);

                // Validar respuesta
                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener productos sustitutos"));
                }

                // Verificar si hay sustitutos - No es error si no hay, simplemente mostramos grid vacío
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridComboRepo", new GridCoreSmart<ComboRepoDto>());
                }

                // Crear grid con los sustitutos
                var grid = new GridCoreSmart<ComboRepoDto>
                {
                    ListaDatos = new StaticPagedList<ComboRepoDto>(respuesta.ListaEntidad, 1, respuesta.ListaEntidad.Count, respuesta.ListaEntidad.Count),
                    CantidadReg = respuesta.ListaEntidad.Count,
                    PrimerRegistro = 1,
                    UltimoRegistro = respuesta.ListaEntidad.Count,
                    RegistroFinal = respuesta.ListaEntidad.Count,
                    CantidadPaginas = 1,
                    PaginaActual = 1,                    
                };

                return PartialView("_gridComboRepo", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al obtener productos sustitutos");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al obtener productos sustitutos");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener productos sustitutos"));
            }
        }

        /// <summary>
        /// Confirma la creación o modificación de una promoción o combo
        /// </summary>
        /// <param name="datos">Datos principales del combo/promoción</param>
        /// <param name="canales">Lista de canales donde aplicará el combo/promoción</param>
        /// <param name="productos">Lista de productos incluidos en el combo/promoción</param>
        /// <returns>Resultado de la operación en formato JSON</returns>
        [HttpPost]
        public async Task<JsonResult> ConfirmacionCombo([FromBody]ConfirmacionRequestDto request)
        {
            try
            {
                // Verificar autenticación - consistente con otros métodos
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "No autorizado" });

                if (request == null)
                {
                    return Json(new { ok = false, mensaje = "Los datos de confirmación no fueron recepcionados. Verifique." });
                }

                // Validaciones de entrada
                if (request.Datos == null)
                {
                    return Json(new { ok = false, mensaje = "Los datos del combo/promoción son requeridos" });
                }

                if (request.Canales == null || !request.Canales.Any())
                {
                    return Json(new { ok = false, mensaje = "Al menos un canal es necesario informar" });
                }

                if (request.Productos == null || !request.Productos.Any())
                {
                    return Json(new { ok = false, mensaje = "Al menos un producto es necesario informar" });
                }

                // Validación adicional de datos principales
                if (string.IsNullOrEmpty(request.Datos.cmb_desc))
                {
                    return Json(new { ok = false, mensaje = "La descripción del combo/promoción es requerida" });
                }

                // Preparar datos para envío
                var prods = request.Productos.Select(x => new { x.p_id, x.cantidad, dto = x.dto_porc, x.activo, costo = x.p_pcosto });
                var sustitutos = ProductosSustitutos ?? new List<ComboSustitutoDto>();
                // Crear un HashSet con los IDs de productos para búsquedas más eficientes (O(1))
                var productosIds = request.Productos.Select(p => p.p_id).ToHashSet();

                // Filtrar sustitutos que solo pertenecen a los productos en la solicitud
                var sus = sustitutos
                    .Where(s => productosIds.Contains(s.p_id))
                    .Select(x => new { x.p_id, x.p_id_sustituto, x.activo, costo = x.p_pcosto });
                var canales = request.Canales.Select(x => new { x.adm_id, x.lp_id }).ToList();
                var req = new AbmPlusGenDto
                {
                    Json = JsonConvert.SerializeObject(prods),
                    Json2 = JsonConvert.SerializeObject(canales),
                    Json3 = JsonConvert.SerializeObject(sus),
                    Json4 = JsonConvert.SerializeObject(request.Datos),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };

                // Llamada al servicio
                var respuesta = await _comboServicio.ConfirmarCombo(req, TokenCookie);

                // Procesamiento de respuesta
                if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
                {
                    // Log y limpieza de datos temporales
                    _logger?.LogInformation("Combo/Promoción guardado exitosamente: {ComboDesc}", request.Datos.cmb_desc);
                    ProductosSustitutos.Clear();

                    // Respuesta de éxito
                    return Json(new
                    {
                        ok = true,
                        error = false,
                        id = respuesta.Entidad?.resultado_id,
                        msg = respuesta.Mensaje ?? (request.Datos.cmb_tipo == 'C' ? 
                               "Combo guardado correctamente" : 
                               "Promoción guardada correctamente")
                    });
                }
                else
                {
                    // Log y respuesta de error/advertencia
                    _logger?.LogWarning("Error en servicio de combo/promoción: {Mensaje}", respuesta.Mensaje);
                    return Json(new
                    {
                        ok = false,
                        id = "",
                        error = respuesta.EsError,
                        warn = respuesta.EsWarn,
                        msg = respuesta.Mensaje ?? "Error al procesar el combo/promoción"
                    });
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones no esperadas
                _logger?.LogError(ex, "Error inesperado al confirmar combo/promoción");
                return Json(new 
                { 
                    ok = false, 
                    error = true,
                    msg = "Error interno al procesar la solicitud" 
                });
            }
        }

        /// <summary>
        /// Elimina un sustituto específico de un producto en la sesión
        /// </summary>
        /// <param name="productoId">ID del producto principal</param>
        /// <param name="sustitutoId">ID del producto sustituto a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost]
        public JsonResult EliminarSustituto(string productoId, string sustitutoId)
        {
            try
            {
                // Validar parámetros
                if (string.IsNullOrEmpty(productoId))
                {
                    return Json(new { ok = false, mensaje = "El ID del producto es obligatorio" });
                }

                if (string.IsNullOrEmpty(sustitutoId))
                {
                    return Json(new { ok = false, mensaje = "El ID del sustituto es obligatorio" });
                }

                // Obtener la lista de relaciones de la sesión
                var listaRelaciones = ProductosSustitutos ?? new List<ComboSustitutoDto>();

                // Contar cuántos sustitutos había antes
                int cantidadAntes = listaRelaciones.Count(r => r.p_id == productoId);

                // Eliminar el sustituto específico
                listaRelaciones.RemoveAll(r => r.p_id == productoId && r.p_id_sustituto == sustitutoId);

                // Contar cuántos quedaron después
                int cantidadDespues = listaRelaciones.Count(r => r.p_id == productoId);

                // Guardar la lista actualizada en sesión
                ProductosSustitutos = listaRelaciones;

                // Verificar si se eliminó algo
                bool seElimino = cantidadAntes > cantidadDespues;

                return Json(new
                {
                    ok = true,
                    mensaje = seElimino 
                        ? $"Sustituto eliminado correctamente. Quedan {cantidadDespues} sustituto(s)" 
                        : "No se encontró el sustituto especificado",
                    cantidadRestante = cantidadDespues,
                    eliminado = seElimino
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al eliminar sustituto");
                return Json(new { ok = false, mensaje = "Error al procesar la solicitud de eliminación" });
            }
        }
    }
}
