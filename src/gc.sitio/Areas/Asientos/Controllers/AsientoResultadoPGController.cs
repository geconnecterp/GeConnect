using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoResultadoPGController : AsientoBaseController
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ASXAJUSTE.ToString();

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoTemporalServicio _asTempSv;
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _appSettings;

        public AsientoResultadoPGController(
          IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
          IHttpContextAccessor contexto,
          ILogger<AsientoTemporalController> logger,
          IAsientoFrontServicio asientoServicio,
          IAsientoTemporalServicio asTempSv,
          IDocManagerServicio docManager
          ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asTempSv = asTempSv;
            _appSettings = options.Value;
            _docsManager = docsManager.Value;
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

                string titulo = "Asiento por Ajuste de Inflación";
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

        [HttpPost]
        public async Task<IActionResult> BuscarAsientosResultado(string eje_nro)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar que el parámetro eje_nro no sea nulo o vacío
                if (string.IsNullOrEmpty(eje_nro))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe seleccionar un ejercicio contable."
                    });
                }

                // Convertir eje_nro a entero para pasarlo al servicio
                if (!int.TryParse(eje_nro, out int ejercicioId))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "El ejercicio seleccionado no es válido."
                    });
                }

                // Llamar al servicio para obtener los asientos de ajuste
                RespuestaGenerica<AsientoResultadoDto> respuesta = await _asientoServicio.ObtenerAsientosPG(ejercicioId, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron asientos de ajuste para el ejercicio seleccionado.";
                    response.Ok = false;
                    response.EsWarn = true;
                    response.EsError = false;
                    return PartialView("_gridMensaje", response);
                }

                // Guardar datos en variable de sesión para uso posterior
                AsientosResultado = respuesta.ListaEntidad;

                // Crear el grid para la vista (sin paginación)
                var grid = GenerarGrillaSmart(
                    respuesta.ListaEntidad,
                    "Ccb_id",  // Ordenamiento por defecto
                    respuesta.ListaEntidad.Count,  // Todos los registros en una página
                    1,  // Página única
                    respuesta.ListaEntidad.Count,  // Total de registros
                    1,  // Total de páginas (una sola)
                    "ASC"  // Dirección de ordenamiento por defecto
                );

                // Configurar leyenda para el grid
                ViewBag.Leyenda = $"Ejercicio {eje_nro} - Asientos de Resultado PG";

                // Devolver la vista parcial con el grid
                return PartialView("_gridres", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener asientos de ajuste");

                response.Mensaje = "Error al obtener los asientos de ajuste.";
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAsientoResultado(int eje_nro, string ccbid, string[] listCcb)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                // Validar parámetros
                if (eje_nro <= 0)
                    return Json(new { error = false, warn = true, msg = "Debe seleccionar un ejercicio contable válido." });

                if (string.IsNullOrEmpty(ccbid))
                    return Json(new { error = false, warn = true, msg = "Debe seleccionar una cuenta de ajuste." });

                if (listCcb == null || listCcb.Length == 0)
                    return Json(new { error = false, warn = true, msg = "Debe seleccionar al menos una cuenta para ajustar." });

                var asientosResultado = AsientosResultado;
                if (asientosResultado == null || !asientosResultado.Any())
                    return Json(new { error = false, warn = true, msg = "No hay datos de asientos disponibles. Intente realizar la búsqueda nuevamente." });

                List<AsientoResultadoDatoDto> asRes = [];
                foreach (var item in listCcb)
                {
                    var reg = asientosResultado.SingleOrDefault(x => x.Ccb_id.Equals(item));
                    if (reg == null)
                    {
                        return Json(new { error = false, warn = true, msg = $"La cuenta seleccionada {item} no se encontró. Intente nuevamente más tarde." });
                    }

                    asRes.Add(new AsientoResultadoDatoDto
                    {
                        eje_nro = eje_nro,
                        ccb_id = item,
                        saldo = reg.Saldo,
                    });
                }


                // Crear objeto para confirmar asiento
                var asientoResGPConfirmar = new AjusteConfirmarDto
                {
                    Json = JsonConvert.SerializeObject(asRes),
                    AdmId = AdministracionId,
                    User = UserName,
                    CcbId = ccbid,
                    EjeNro = eje_nro
                };

                // Enviar al servicio
                var res = await _asientoServicio.ConfirmarAsientoResultadoPG(asientoResGPConfirmar, TokenCookie);

                if (res.Ok)
                {
                    return Json(new
                    {
                        error = false,
                        warn = false,
                        msg = "EL ALTA DEL ASIENTO DE RESULTADO GP SE REALIZÓ SATISFACTORIAMENTE"
                    });
                }
                else if (res.Entidad != null)
                {
                    return Json(new
                    {
                        error = false,
                        warn = true,
                        msg = res.Entidad.resultado_msj,
                        focus = res.Entidad.resultado_setfocus
                    });
                }
                else
                {
                    return Json(new
                    {
                        error = false,
                        warn = true,
                        msg = "Hubo un problema al intentar confirmar el asiento de ajuste."
                    });
                }
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "Excepción de negocio al confirmar asiento de ajuste");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "Error de autorización al confirmar asiento de ajuste");
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al confirmar asiento de ajuste");
                return Json(new { error = true, warn = false, msg = $"Error al procesar la solicitud: {ex.Message}" });
            }
        }
    }
}
