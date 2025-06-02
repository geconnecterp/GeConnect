using DocumentFormat.OpenXml.Drawing;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using IronSoftware.DOM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoTemporalController : AsientoBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.ASTEMP.ToString();

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoTemporalServicio _asTempSv;
        private readonly IAbmServicio _abmSv;
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _appSettings;


        public AsientoTemporalController(
            IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
            IHttpContextAccessor contexto,
            ILogger<AsientoTemporalController> logger,
            IAsientoFrontServicio asientoServicio,
            IAsientoTemporalServicio asTempSv,
            IAbmServicio abm,
            IDocManagerServicio docManager
            ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asTempSv = asTempSv;
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

                string titulo = "Asientos Temporales";
                ViewData["Titulo"] = titulo;

                #region Gestor Impresion - Inicializacion de variables
                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                // en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                // Obtenemos los datos para los combos
                await ObtenerEjerciciosContables();
                await ObtenerTiposAsiento();
                await ObtenerUsuariosDeEjercicio(0); // Cargamos los usuarios para ejercicio 0

                // Asignamos los combos a la vista
                ViewBag.ListaEjercicios = ComboEjercicios();
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();
                ViewBag.ListaUsuarios = ComboUsuariosEjercicio();
                
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de Asientos Temporales");
                TempData["error"] = "Hubo un problema al cargar la vista de Asientos Temporales. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        /// <summary>
        /// Pasa los asientos temporales seleccionados a contabilidad definitiva
        /// </summary>
        /// <param name="asientosIds">Array con los IDs de los asientos a pasar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost]
        public async Task<JsonResult> PasarAContabilidad([FromBody] PaseAContabilidadDto datos) // List<string> asientosIds, int eje_nro
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new { error = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                // Validar que existan asientos seleccionados
                if (datos.asientosIds == null || !datos.asientosIds.Any())
                {
                    return Json(new { error = true, msg = "Debe seleccionar al menos un asiento para pasar a contabilidad." });
                }

                var asientoPasa = new AsientoPasaDto
                {
                    Eje_nro = datos.eje_nro,
                    JsonDiaMovi = JsonConvert.SerializeObject(datos.asientosIds), 
                    Usu_id = UserName, // Usuario actual
                    Adm_id = AdministracionId // Administración actual
                };

                // Llamar al servicio para realizar el traspaso
                var response = await _asTempSv.PasarAsientosAContabilidad(asientoPasa, TokenCookie);

                if (!response.Ok || response.ListaEntidad == null)
                {
                    return Json(new
                    {
                        error = true,
                        msg = response.Mensaje ?? "Error al pasar los asientos a contabilidad."
                    });
                }

                // Analizar los resultados de cada asiento
                var resultados = response.ListaEntidad;
                var asientosExitosos = resultados.Count(r => r.resultado == 0);
                var asientosConError = resultados.Where(r => r.resultado != 0).ToList();

                // Todos los asientos fueron procesados exitosamente
                if (asientosConError.Count == 0)
                {
                    return Json(new
                    {
                        error = false,
                        msg = $"Los {asientosExitosos} asiento(s) seleccionado(s) han sido pasados a contabilidad con éxito."
                    });
                }
                // Algunos asientos fueron procesados exitosamente, otros con error
                else if (asientosExitosos > 0)
                {
                    // Preparar los detalles de error para enviarlos al cliente
                    var detallesError = asientosConError.Select(r => new {
                        asientoId = r.resultado_id,
                        mensaje = r.resultado_msj
                    }).ToList();

                    return Json(new
                    {
                        error = true,
                        parcial = true,  // Indica que hay asientos procesados correctamente
                        exitosos = asientosExitosos,
                        fallidos = asientosConError.Count,
                        msg = $"Se procesaron {asientosExitosos} asiento(s) correctamente, pero {asientosConError.Count} asiento(s) presentaron errores.",
                        detalles = detallesError
                    });
                }
                // Todos los asientos presentaron errores
                else
                {
                    // Si son pocos errores, se pueden mostrar directamente
                    if (asientosConError.Count <= 3)
                    {
                        var mensajesError = string.Join("<br>", asientosConError.Select(r =>
                            $"Asiento {r.resultado_id}: {r.resultado_msj}"));

                        return Json(new
                        {
                            error = true,
                            msg = $"No se pudo procesar ningún asiento. Errores:<br>{mensajesError}"
                        });
                    }
                    // Si son muchos errores, enviar solo el conteo y detalles para posible reporte
                    else
                    {
                        var detallesError = asientosConError.Select(r => new {
                            asientoId = r.resultado_id,
                            mensaje = r.resultado_msj
                        }).ToList();

                        return Json(new
                        {
                            error = true,
                            muchos = true, // Indica que hay muchos errores para mostrar
                            fallidos = asientosConError.Count,
                            msg = $"No se pudo procesar ningún asiento. Se encontraron {asientosConError.Count} errores.",
                            detalles = detallesError
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al pasar asientos a contabilidad");
                return Json(new
                {
                    error = true,
                    msg = "Se produjo un error al procesar la solicitud: " + ex.Message
                });
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
                var response = await _asTempSv.ObtenerAsientos(query, TokenCookie);

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
            catch (Exception ex)
            {
                // Registrar el error y devolver un mensaje genérico
                _logger?.LogError(ex, "Error al buscar asientos temporales.");
                return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                {
                    Ok = false,
                    Mensaje = "Ocurrió un error inesperado al buscar los asientos temporales."
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
                await ObtenerTiposAsiento();
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();

                // Llamar al servicio para obtener el detalle del asiento
                var response = await _asTempSv.ObtenerAsientoDetalle(id, TokenCookie);

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

        [HttpPost]
        public async Task<JsonResult> ConfirmarAsientoTemporal([FromBody] AsientoAccionDto datos)
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
                    Objeto = "asientotmp",
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

        private List<AsientoPlanoDto> ConvertirAsientoAPlano(AsientoDetalleDto asiento)
        {

            var asientosPlanos = new List<AsientoPlanoDto>();

            foreach (var linea in asiento.Detalles)
            {
                var asientoPlano = new AsientoPlanoDto
                {
                    dia_movi = asiento.Dia_movi,
                    dia_fecha = asiento.Dia_fecha,
                    dia_tipo = asiento.Dia_tipo,
                    dia_lista = asiento.Dia_lista,
                    dia_desc_asiento = asiento.Dia_desc_asiento,
                    dia_nro = linea.Dia_nro,
                    ccb_id = linea.Ccb_id,
                    ccb_desc = linea.Ccb_desc,
                    dia_desc = linea.Dia_desc,
                    debe = linea.Debe,
                    haber = linea.Haber
                };

                asientosPlanos.Add(asientoPlano);
            }

            return asientosPlanos;
        }

        /// <summary>
        /// Obtiene los ejercicios contables para el combo
        /// </summary>
        private async Task ObtenerEjerciciosContables()
        {
            try
            {
                var response = await _asientoServicio.ObtenerEjercicios(TokenCookie);
                if (response.Ok && response.ListaEntidad != null)
                {
                    ViewBag.EjerciciosLista = response.ListaEntidad.OrderByDescending(x=> x.Eje_desde).ToList();
                }
                else
                {
                    ViewBag.EjerciciosLista = new List<EjercicioDto>();
                    _logger?.LogWarning($"No se pudieron obtener los ejercicios contables: {response.Mensaje}");
                }
            }
            catch (Exception ex)
            {
                ViewBag.EjerciciosLista = new List<EjercicioDto>();
                _logger?.LogError(ex, "Error al obtener ejercicios contables");
            }
        }

        /// <summary>
        /// Obtiene los tipos de asiento para el combo
        /// </summary>
        private async Task ObtenerTiposAsiento()
        {
            try
            {
                var response = await _asientoServicio.ObtenerTiposAsiento(TokenCookie);
                if (response.Ok && response.ListaEntidad != null)
                {
                    ViewBag.TiposAsientoLista = response.ListaEntidad;
                }
                else
                {
                    ViewBag.TiposAsientoLista = new List<TipoAsientoDto>();
                    _logger?.LogWarning($"No se pudieron obtener los tipos de asiento: {response.Mensaje}");
                }
            }
            catch (Exception ex)
            {
                ViewBag.TiposAsientoLista = new List<TipoAsientoDto>();
                _logger?.LogError(ex, "Error al obtener tipos de asiento");
            }
        }

        /// <summary>
        /// Obtiene los usuarios de un ejercicio específico
        /// </summary>
        private async Task ObtenerUsuariosDeEjercicio(int eje_nro)
        {
            try
            {
                var response = await _asientoServicio.ObtenerUsuariosDeEjercicio(eje_nro, TokenCookie);
                if (response.Ok && response.ListaEntidad != null)
                {
                    UsuariosEjercicioLista = response.ListaEntidad;
                }
                else
                {
                    UsuariosEjercicioLista = new List<UsuAsientoDto>();
                    _logger?.LogWarning($"No se pudieron obtener los usuarios del ejercicio {eje_nro}: {response.Mensaje}");
                }
            }
            catch (Exception ex)
            {
                UsuariosEjercicioLista = new List<UsuAsientoDto>();
                _logger?.LogError(ex, $"Error al obtener usuarios del ejercicio {eje_nro}");
            }
        }

        

        /// <summary>
        /// Método AJAX para obtener los usuarios de un ejercicio específico
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CargarUsuariosEjercicio(int eje_nro)
        {

            if (eje_nro <= 0)
            {
                return Json(new { success = false, message = "El número de ejercicio no es válido." });
            }

            try
            {
                await ObtenerUsuariosDeEjercicio(eje_nro);

                if (UsuariosEjercicioLista == null || !UsuariosEjercicioLista.Any())
                {
                    return Json(new { success = false, message = "No se encontraron usuarios para el ejercicio especificado." });
                }

                var usuarios = UsuariosEjercicioLista.Select(u => new {
                    id = u.Usu_id,
                    text = u.Usu_apellidoynombre
                }).ToList();

                return Json(new { success = true, data = usuarios });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al obtener usuarios del ejercicio {eje_nro}");
                return Json(new { success = false, message = "Ocurrió un error inesperado al obtener los usuarios del ejercicio." });
            }
        }
        /// <summary>
        /// Prepara un asiento temporal vacío para crear uno nuevo
        /// </summary>
        /// <returns>Vista parcial con un asiento temporal vacío</returns>
        [HttpPost]
        public async Task<IActionResult> NuevoAsiento()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Cargar los tipos de asiento para el dropdown
                await ObtenerTiposAsiento();
                ViewBag.ListaTiposAsiento = ComboTiposAsiento();

                // Crear un nuevo objeto AsientoDetalleDto vacío
                var asientoVacio = new AsientoDetalleDto
                {
                    Dia_movi = string.Empty,
                    Dia_fecha = DateTime.Now,
                    Dia_tipo = string.Empty,
                    Dia_lista = string.Empty,
                    Dia_desc_asiento = string.Empty,
                    TotalDebe = 0,
                    TotalHaber = 0,
                    Detalles = new List<AsientoLineaDto>()
                };

                // Agregar una línea en blanco por defecto
                asientoVacio.Detalles.Add(new AsientoLineaDto
                {
                    Dia_movi = string.Empty,
                    Dia_nro = 1,
                    Ccb_id = string.Empty,
                    Ccb_desc = string.Empty,
                    Dia_desc = string.Empty,
                    Debe = 0,
                    Haber = 0
                });

                // Retornar la vista parcial con el asiento vacío
                return PartialView("_asiento", asientoVacio);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear un nuevo asiento temporal");
                return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                {
                    Ok = false,
                    Mensaje = "Ocurrió un error inesperado al crear un nuevo asiento."
                });
            }
        }


       

    }
}
