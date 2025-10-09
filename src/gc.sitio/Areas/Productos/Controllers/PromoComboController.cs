using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        public async Task<IActionResult> PresentarPromosYCombos(QueryFilters filtros,int pag=1)
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
                // Crear lista paginada
                var pagedList = new StaticPagedList<ComboListaDto>(
                    combos,
                    pag,
                    registrosPorPagina,
                    cantTotalReg
                );

                // Configurar el GridCoreSmart
                var grid = new GridCoreSmart<ComboListaDto>
                {
                    ListaDatos = pagedList, //lista de combos
                    CantidadReg = combos.Count, //cantidad actual de registros
                    PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
                    UltimoRegistro = Math.Min(pag * registrosPorPagina, combos.Count), //define cual es el ultimo registro
                    RegistroFinal = combos.Count, //indica cual es el ultimo registro
                    CantidadPaginas = (int)Math.Ceiling((double)combos.Count / registrosPorPagina),//calcula la cantidad de paginas
                    PaginaActual = pag,//especifica que pagina es la actual
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
    }
}
