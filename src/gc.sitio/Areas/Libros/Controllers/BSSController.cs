using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Libros.Controllers
{
    [Area("Libros")]
    public class BSSController : MayorBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.BSS.ToString();
        private readonly AppSettings _appSettings;

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IBSSServicio _bssServicio;
        private readonly IDocManagerServicio _docMSv;
        public BSSController(
            IOptions<AppSettings> options,
            IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<BSSController> logger,
            IDocManagerServicio docManager,
            IBSSServicio bSSServicio,
            IAsientoFrontServicio asientoFront) : base(options, contexto, logger)
        {
            _appSettings = options.Value;
            _docsManager = docsManager.Value;
            _asientoServicio = asientoFront;
            _bssServicio = bSSServicio;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Balance de Sumas y Saldos";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables
                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                // en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                // Obtenemos los datos para los combos
                await ObtenerEjerciciosContables(_asientoServicio);

                // Asignamos los combos a la vista
                ViewBag.ListaEjercicios = ComboEjercicios();

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        /// <summary>
        /// Obtiene la lista de ejercicios contables para cargar el combo en la vista
        /// </summary>
        /// <returns>Lista de ejercicios en formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> ObtenerEjercicios()
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Obtener token de autenticación
                string token = TokenCookie;
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { error = true, msg = "No se encontró un token de autenticación válido." });
                }

                // Llamar al servicio para obtener los ejercicios
                var response = await _asientoServicio.ObtenerEjercicios(token);

                // Validar si hay error en la respuesta
                if (response.EsError)
                {
                    return Json(new { error = true, msg = response.Mensaje });
                }

                // Obtener la lista de ejercicios contables
                var ejercicios = response.ListaEntidad ?? new List<EjercicioDto>();

                // Preparar datos para la respuesta
                var resultado = ejercicios.OrderByDescending(x => x.Eje_ctl).Select(e => new
                {
                    eje_nro = e.Eje_nro,
                    eje_lista = e.Eje_lista
                }).ToList();

                return Json(resultado);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener ejercicios contables");
                return Json(new { error = true, msg = "Error al obtener ejercicios contables: " + ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los datos del BALANCE de SS según los filtros especificados
        /// </summary>
        /// <param name="query">Parámetros de filtro</param>
        /// <returns>Vista parcial con el grid o mensaje de error</returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerBSS(LibroFiltroDto query, string sort = "Eje_nro", string sortDir = "asc", int pagina = 1)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar el filtro recibido
                if (query == null)
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "El filtro no puede ser nulo."
                    });
                }

                // Validar parámetros obligatorios
                if (query.eje_nro <= 0)
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe seleccionar un ejercicio contable válido."
                    });
                }


                // Configurar los parámetros de paginación y ordenamiento
                query.Sort = sort;
                query.SortDir = sortDir;
                query.Registros = _appSettings.NroRegistrosPagina;
                query.Pagina = pagina;

                // Llamar al servicio para obtener el libro mayor
                var res = await _bssServicio.ObtenerBalanceSumaSaldos(query, Token);

                if (res.Item1.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros para el Balance de Sumas y Saldos en el Ejercicio {query.eje_nro}");
                }

                BalanceSS = res.Item1; // Asignar la lista de Libro Mayor
                var lista = res.Item1.OrderBy(x => x.Ccb_id).ToList();
                MetadataGeneral = res.Item2;

                // Crear el grid para la vista
                var grid = GenerarGrillaSmart(
                    lista,
                    query.Sort,
                    _appSettings.NroRegistrosPagina,
                    pagina,
                    lista.Count,
                     (int)Math.Ceiling((double)lista.Count / _appSettings.NroRegistrosPagina), // Total de páginas
                    query.SortDir
                );
                string leyenda = $"Ejercicio {query.eje_nro} - Desde {query.desde.ToShortDateString()} al {query.hasta.ToShortDateString()} - Balance de Sumas y Saldos";
               
                ViewBag.Leyenda = $"{leyenda}";
                return PartialView("_gridBalanceSS", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, ex.Message);

                string msg = ex.Message;
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el Balance SS: {Mensaje}", ex.Message);

                string msg = "Error al obtener el Balance de SS";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
    }
}
