using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoDefinitivoController : AsientoBaseController
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ASTEMP.ToString();

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoDefinitivoServicio _asDefSv;
        private readonly IAbmServicio _abmSv;
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _appSettings;

        public AsientoDefinitivoController(
            IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<AsientoDefinitivoController> logger,
            IAsientoFrontServicio asientoServicio,
            IAsientoDefinitivoServicio asDefSv,
            IAbmServicio abm,
            IDocManagerServicio docManager
            ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asDefSv = asDefSv;
            _appSettings = options.Value;
            _abmSv = abm;
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

                string titulo = "Asientos DEFINITIVOS";
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
                await ObtenerTiposAsiento(_asientoServicio);
                if (EjercicioSeleccionado > 0)
                {
                    await ObtenerUsuariosDeEjercicio(EjercicioSeleccionado, _asientoServicio); // Cargamos los usuarios para ejercicio 0
                }

                // Indicar que estamos en el contexto de asientos definitivos
                ViewBag.EsAsientoDefinitivo = true;

                // Asignamos los combos a la vista
                ViewBag.ListaEjercicios = ComboEjercicios();
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();
                ViewBag.ListaUsuarios = ComboUsuariosEjercicio();

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de Asientos Definitivos");
                TempData["error"] = "Hubo un problema al cargar la vista de Asientos Definitivos. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarAsientos(QueryAsiento query, string sort = "Eje_nro", string sortDir = "asc", int pag = 1)
        {
            try
            {
                // Versión optimizada del código de autenticación
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

                // Configurar los parámetros de paginación y ordenamiento
                query.Sort = sort;
                query.SortDir = sortDir;
                query.TotalRegistros = _appSettings.NroRegistrosPagina;
                query.Paginas = pag;

                // Llamar al servicio para obtener los asientos temporales
                var response = await _asDefSv.ObtenerAsientos(query, TokenCookie);

                var lista = response.Item1;
                MetadataGeneral = response.Item2;

                // Generar el objeto GridCoreSmart<T>
                var grillaDatos = GenerarGrillaSmart(
                    lista,
                    sort,
                    _appSettings.NroRegistrosPagina,
                    pag,
                    lista.Count, // Total de registros
                    (int)Math.Ceiling((double)lista.Count / _appSettings.NroRegistrosPagina), // Total de páginas
                    sortDir
                );

                // Retornar la vista parcial con los datos
                return PartialView("_gridAsiento", grillaDatos);
            }
            catch (NegocioException ex)
            {
                // Registrar el error y devolver un mensaje genérico
                _logger?.LogError(ex, "Error al buscar asientos Definitivos.");
                return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                {
                    Ok = false,
                    Mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Registrar el error y devolver un mensaje genérico
                _logger?.LogError(ex, "Error al buscar asientos Definitivos.");
                return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                {
                    Ok = false,
                    Mensaje = "Ocurrió un error inesperado al buscar los asientos definitivos."
                });
            }
        }

        /// <summary>
        /// Obtiene el detalle de un asiento temporal específico
        /// </summary>
        /// <param name="id">Identificador del asiento (número de movimiento)</param>
        /// <returns>Vista parcial con el detalle del asiento</returns>
        [HttpPost]
        public async Task<IActionResult> BuscarAsiento(string id)
        {
            try
            {

                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (string.IsNullOrEmpty(id))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "El identificador del asiento no puede estar vacío."
                    });
                }

                // Cargar los tipos de asiento para el dropdown
                await ObtenerTiposAsiento(_asientoServicio);
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();

                // Llamar al servicio para obtener el detalle del asiento
                var response = await _asDefSv.ObtenerAsientoDetalle(id, TokenCookie);

                if (!response.Ok || response.Entidad == null)
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = response.Mensaje ?? "No se encontró información del asiento solicitado."
                    });
                }

                // Retornar la vista parcial con los datos
                return PartialView("_asiento", response.Entidad);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al obtener el detalle del asiento {id}");
                return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                {
                    Ok = false,
                    Mensaje = "Ocurrió un error inesperado al obtener el detalle del asiento."
                });
            }
        }

        /// <summary>
        /// Verifica si un asiento definitivo puede ser modificado basado en su fecha y la fecha de control del ejercicio
        /// </summary>
        /// <param name="data">Objeto con el número de ejercicio y la fecha del asiento</param>
        /// <returns>Resultado de la verificación</returns>
        [HttpPost]
        public JsonResult VerificarFechaModificacion([FromBody] VerificacionFechaDto data)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new { permitido = false, mensaje = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                if (data == null || data.eje_nro <= 0)
                {
                    return Json(new
                    {
                        permitido = false,
                        mensaje = "Se requiere especificar el ejercicio contable."
                    });
                }

                if (data.dia_fecha == DateTime.MinValue)
                {
                    return Json(new
                    {
                        permitido = false,
                        mensaje = "Se requiere especificar una fecha válida para el asiento."
                    });
                }

                //Tenemos el listado de ejercicios y cada uno de ellos tiene el intervalo de tiempo
                var lista = Ejercicios;
                var ejer = lista.Find(x => x.Eje_nro.ToInt() == data.eje_nro);
                if (ejer == null)
                {
                    return Json(new
                    {
                        permitido = false,
                        mensaje = "No se pudo identificar el ejercicio contable."
                    });
                }


                // Llamar al servicio para verificar si la fecha es válida para modificación
                //var resultado = await _asDefSv.VerificarFechaModificacion(data.eje_nro, data.dia_fecha, TokenCookie);

                if(ejer.Eje_ctl< data.dia_fecha)
                {
                    return Json(new
                    {
                        permitido = false,
                        mensaje = "No se puede modificar el asiento debido a restricciones de fecha."
                    });
                }

                // Si llegamos aquí, está permitido modificar
                return Json(new
                {
                    permitido = true,
                    mensaje = "El asiento puede ser modificado."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al verificar la fecha de modificación");
                return Json(new
                {
                    permitido = false,
                    mensaje = "Ocurrió un error inesperado al verificar si el asiento puede ser modificado."
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAsientoDefinitivo([FromBody] AsientoAccionDto datos)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var asiento = datos.asiento;
                var accion = datos.accion;

                asiento = HelperGen.PasarAMayusculas(asiento);

                var asientoPlano = ConvertirAsientoAPlano(asiento);

                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(asientoPlano),
                    Objeto = "asientodef",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = accion
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"EL ALTA DEL ASIENTO SE PROCESO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"LA MODIFICIACION DEL ASIENTO SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"LA BAJA DEL ASIENTO SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    VendedoresLista = [];

                    if (abm.Abm.Equals('A') && res.Entidad != null)
                    {
                        return Json(new { error = false, warn = false, msg, id = res.Entidad.resultado_id });
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    if (res.Entidad != null)
                    {
                        return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
                    }
                    else
                    {
                        return Json(new { error = false, warn = true, msg = "Hubo un problema al intentar confirmar al vendedor." });
                    }
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerUsuariosEjercicio(int ejercicioId)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Obtener los usuarios del ejercicio
                await ObtenerUsuariosDeEjercicio(ejercicioId, _asientoServicio);

                // Devolver la lista en formato JSON para select2
                return Json(ComboUsuariosEjercicio().Items);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al obtener usuarios del ejercicio {ejercicioId}");
                return StatusCode(500, "Error interno al obtener usuarios del ejercicio");
            }
        }

    }
}
