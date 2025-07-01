using DocumentFormat.OpenXml.Math;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers
{
    [Area("Libros")]
    public class EjercicioController : ControladorBase
    {
        private readonly AppSettings _appSettings;

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAbmServicio _abmSv;

        public EjercicioController(
            IOptions<AppSettings> options,
            IHttpContextAccessor contexto,
            ILogger<EjercicioController> logger,
            IDocManagerServicio docManager,
            IBalanceGrServicio bgrServicio,
            IAbmServicio abmServicio,
            IAsientoFrontServicio asientoFront) : base(options, contexto, logger)
        {
            _appSettings = options.Value;
            _asientoServicio = asientoFront;
            _abmSv = abmServicio;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Ejercicio Contable";
                ViewData["Titulo"] = titulo;
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
        public IActionResult ObtenerDatosEjercicio(string ejercicioId)
        {
            try
            {
                var ejercicio = Ejercicios.FirstOrDefault(e => e.Eje_nro.ToString() == ejercicioId);

                if (ejercicio == null)
                {
                    return PartialView("_MensajeError", "No se encontró el ejercicio solicitado.");
                }

                return PartialView("_ejercicio", ejercicio);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener datos del ejercicio {EjercicioId}", ejercicioId);
                return PartialView("_MensajeError", "Ocurrió un error al cargar los datos del ejercicio.");
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarEjercicio(EjercicioDto dto, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (accion != 'A' && accion != 'M')
                {
                    return Json(new { error = false, warn = true, msg = "Acción no válida. Debe ser 'A' para alta o 'M' para modificación." });
                }

                var json = new EjerResponseDto
                {
                    eje_nro = dto.Eje_nro.ToInt(),
                    eje_desde = dto.Eje_desde,
                    eje_hasta = dto.Eje_hasta.AddHours(23).AddMinutes(59).AddSeconds(59),
                    eje_ctl = dto.Eje_ctl
                };
                var confirmar = new AbmGenDto
                {
                    Json = JsonConvert.SerializeObject(json),
                    Objeto = "ejercicio",
                    Abm = accion,
                    Usuario = UserName,
                    Administracion = AdministracionId
                };

                var res = await _abmSv.AbmConfirmar(confirmar, TokenCookie);

                if (res.Ok)
                {
                    string msg = string.Empty;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL EJERCICIO {dto.Eje_nro} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL EJERCICIO {dto.Eje_nro} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    // Obtenemos los datos para los combos
                    await ObtenerEjerciciosContables(_asientoServicio);

                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad?.resultado_msj, focus = res.Entidad?.resultado_setfocus });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al actualizar el ejercicio {dto.Eje_nro}");
                return Json(new { error = true, msg = "Ocurrió un error al actualizar el ejercicio." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> NuevoEjercicio()
        {
            try
            {
                var ejerc = Ejercicios.Where(x => x.Eje_activo == 'S').FirstOrDefault();

                if (ejerc == null)
                {
                    // Llamar al servicio para obtener los ejercicios
                    var response = await _asientoServicio.ObtenerEjercicios(TokenCookie);

                    // Validar si hay error en la respuesta
                    if (response.EsError)
                    {
                        return Json(new { error = true, msg = response.Mensaje });
                    }

                    // Obtener la lista de ejercicios contables
                    Ejercicios = response.ListaEntidad ?? new List<EjercicioDto>();
                    ejerc = Ejercicios.Where(x => x.Eje_activo == 'S').FirstOrDefault();
                    if (ejerc == null)
                    {
                        throw new NegocioException("No se logro identificar el ejercicio activo. Intentelo de nuevo más tarde. Si el problema persiste avise al Administrador del Sistema.");
                    }
                }

                var nuevoEjercicio = new EjercicioDto
                {
                    Eje_nro = "0", // ID temporal para nuevo ejercicio
                    Eje_desde = ejerc.Eje_hasta.AddDays(1), // Fecha actual como fecha de inicio
                    Eje_hasta = ejerc.Eje_hasta.AddYears(1), // Un año después menos un día
                    Eje_ctl = ejerc.Eje_hasta.AddDays(1), // Fecha actual como fecha de control
                    Eje_activo = 'N' // Nuevo ejercicio será activo
                };

                return PartialView("_ejercicio", nuevoEjercicio);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al preparar formulario para nuevo ejercicio");
                return PartialView("_MensajeError", "Ocurrió un error al preparar el formulario para un nuevo ejercicio.");
            }
        }
    }
}
