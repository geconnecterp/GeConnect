using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Asientos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace gc.sitio.Areas.Libros.Controllers
{
    [Area("Libros")]
    public class MayorController : MayorBase
    {
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.LMAYOR.ToString();
        private readonly AppSettings _appSettings;

        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly ILibroMayorServicio _libroMayorServicio;
        private readonly IDocManagerServicio _docMSv;
        private readonly IABMPlanCuentaServicio _pcuentaSv;
        private readonly ILibroDiarioServicio _ldiarioServicio;

        public MayorController(
              IOptions<AppSettings> options, IOptions<DocsManager> docsManager,
              IHttpContextAccessor contexto,
              ILogger<MayorController> logger,
              IAsientoFrontServicio asientoServicio,
              ILibroMayorServicio libroMayorServicio,
              IDocManagerServicio docManager,
              IABMPlanCuentaServicio cuentaServicio,
              ILibroDiarioServicio libroDiarioServicio
            ) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _libroMayorServicio = libroMayorServicio;
            _appSettings = options.Value;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManager;
            _pcuentaSv = cuentaServicio;
            _ldiarioServicio = libroDiarioServicio; 
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Libro MAYOR";
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
                _logger?.LogError(ex, "Error al cargar la vista de LIBRO MAYOR");
                TempData["error"] = "Hubo un problema al cargar la vista de LIBRO MAYOR. Si el problema persiste, contacte al administrador.";
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
        /// Obtiene los datos del Libro Mayor según los filtros especificados
        /// </summary>
        /// <param name="query">Parámetros de filtro</param>
        /// <returns>Vista parcial con el grid o mensaje de error</returns>
        [HttpPost]
        public async Task<IActionResult> ObtenerLibroMayor(LMayorFiltroDto query, string sort = "Eje_nro", string sortDir = "asc", int pagina = 1)
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

                //if (string.IsNullOrEmpty(query.ccb_id))
                //{
                //    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                //    {
                //        Ok = false,
                //        Mensaje = "Debe seleccionar una cuenta contable válida."
                //    });
                //}

                // Configurar los parámetros de paginación y ordenamiento
                query.Sort = sort;
                query.SortDir = sortDir;
                query.Registros = _appSettings.NroRegistrosPagina;
                query.Pagina = pagina;

                // Llamar al servicio para obtener el libro mayor
                var res = await _libroMayorServicio.ObtenerLibroMayor(query, Token);

                if (res.Item1.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros para la cuenta {query.ccb_id} - {query.ccb_desc}");
                }

                LibroMayor = res.Item1; // Asignar la lista de Libro Mayor
                var lista = res.Item1.OrderBy(x=>x.Dia_fecha).ToList();
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
                string leyenda = $"Ejercicio {query.eje_nro} - Cuenta {query.ccb_id} {query.ccb_desc}";
                string periodo = query.rango ? $"{query.desde.ToShortDateString()} al {query.hasta.ToShortDateString()}" : "";
                ViewBag.Leyenda = $"{leyenda} {periodo}";
                return PartialView("_gridLibroMayor", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex,  ex.Message);

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
                _logger?.LogError(ex, "Error al obtener libro mayor: {Mensaje}", ex.Message);
                
                string msg = "Error al obtener libro mayor";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        /// <summary>
        /// Obtiene el acumulado por día del Libro Mayor desde la variable de sesión
        /// </summary>
        /// <returns>Vista parcial con el grid de acumulados por día</returns>
        [HttpGet]
        public IActionResult LMAcumuladoXDia()
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Obtener datos desde la variable de sesión
                var lista = LibroMayor;

                if (lista == null || !lista.Any())
                {
                    return PartialView("_gridMensaje", "No hay datos disponibles. Por favor, realice una consulta primero.");
                }

                // Agrupar datos por fecha y calcular totales
                var acumuladoPorDia = lista
                    .GroupBy(r => r.Dia_fecha.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key,
                        FechaStr = g.Key.ToString("dd/MM/yyyy"),
                        TotalDebe = g.Sum(r => r.Dia_debe),
                        TotalHaber = g.Sum(r => r.Dia_haber),
                        SaldoFinal = g.OrderBy(r => r.Dia_fecha).ThenBy(r => r.Dia_movi).Last().Dia_saldo
                    })
                    .OrderBy(r => r.Fecha)
                    .ToList();

                // Crear un DTO para el acumulado
                var acumuladoDto = acumuladoPorDia.Select(a => new LMayorAcumuladoDiaDto
                {
                    fecha = a.Fecha,
                    fecha_str = a.FechaStr,
                    total_debe = a.TotalDebe,
                    total_haber = a.TotalHaber,
                    saldo_final = a.SaldoFinal,
                    // Asignar el saldo anterior del primer registro
                    //saldo_anterior = lista.FirstOrDefault()?.saldo_anterior ?? 0
                }).ToList();

                // Generar la grilla
                var grid = GenerarGrillaSmart(
                    acumuladoDto,
                    "fecha",
                    1000, // No paginar esta vista
                    1,
                    acumuladoDto.Count,
                    1,
                    "ASC"
                );

                return PartialView("_gridLMAcum", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en LMAcumuladoXDia: {Mensaje}", ex.Message);
                return PartialView("_gridMensaje", $"Error al procesar los datos: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el detalle de movimientos para una fecha específica desde la variable de sesión
        /// </summary>
        /// <param name="fecha">Fecha para la cual obtener detalle (formato dd/MM/yyyy)</param>
        /// <returns>Vista parcial con el detalle del día</returns>
        [HttpGet]
        public IActionResult LMAcumuladoXDiaDetalle(string fecha)
        {
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (string.IsNullOrEmpty(fecha))
                {
                    return PartialView("_gridMensaje", "Debe especificar una fecha.");
                }

                // Convertir el string de fecha a DateTime
                if (!DateTime.TryParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime fechaSeleccionada))
                {
                    return PartialView("_gridMensaje", "El formato de fecha no es válido.");
                }

                // Obtener datos desde la variable de sesión
                var lista = LibroMayor;

                if (lista == null || lista.Count == 0)
                {
                    return PartialView("_gridMensaje", "No hay datos disponibles.");
                }

                // Filtrar solo los registros del día seleccionado
                var registrosDia = lista
                    .Where(r => r.Dia_fecha.Date == fechaSeleccionada.Date)
                    .OrderBy(r => r.Dia_movi)
                    .ToList();

                if (registrosDia.Count == 0)
                {
                    return PartialView("_gridMensaje", $"No hay registros para la fecha {fecha}.");
                }

                // Pasar la fecha como ViewBag para mostrarla en el encabezado
                ViewBag.FechaSeleccionada = fecha;

                // Generar la grilla
                var grid = GenerarGrillaSmart(
                    registrosDia,
                    "dia_movi",
                    1000, // No paginar esta vista
                    1,
                    registrosDia.Count,
                    1,
                    "ASC"
                );

                return PartialView("_gridLMAcumDet", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en LMAcumuladoXDiaDetalle: {Mensaje}", ex.Message);
                return PartialView("_gridMensaje", $"Error al procesar los datos: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerAsientosLibroDiario(LDiarioRequest query)
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
                if (query.Eje_nro <= 0)
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe seleccionar un ejercicio contable válido."
                    });
                }

               
                query.Regs= _appSettings.NroRegistrosPagina;
        

                // Llamar al servicio para obtener el libro mayor
                var res = await _ldiarioServicio.ObtenerAsientosLibroDiario(query, Token);

                if (res.Item1.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros para los movimientos seleccionados.");
                }

                LibroDiario = res.Item1; // Asignar la lista de Libro Mayor
                var lista = res.Item1.OrderBy(x => x.Dia_movi).ToList();
                MetadataGeneral = res.Item2;

                // Crear el grid para la vista
                var grid = GenerarGrillaSmart(
                    lista,
                    "dia_movi",
                    _appSettings.NroRegistrosPagina,
                    query.Pag,
                    lista.Count,
                     (int)Math.Ceiling((double)lista.Count / _appSettings.NroRegistrosPagina), // Total de páginas
                    string.Empty 
                );
                
                return PartialView("_gridLibroDiario", grid);
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
                _logger?.LogError(ex, "Error al obtener libro mayor: {Mensaje}", ex.Message);

                string msg = "Error al obtener libro mayor";
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
