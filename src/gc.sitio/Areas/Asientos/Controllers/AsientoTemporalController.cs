using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Asientos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoTemporalController : ControladorBase
    {
        private readonly IAsientoFrontServicio _asientoServicio;
        private readonly IAsientoTemporalServicio _asTempSv;
        private readonly AppSettings _appSettings;

        private List<UsuAsientoDto> UsuariosEjercicioLista { get; set; } = [];

        public AsientoTemporalController(
            IOptions<AppSettings> options,
            IHttpContextAccessor contexto,
            ILogger<AsientoTemporalController> logger,
            IAsientoFrontServicio asientoServicio,
            IAsientoTemporalServicio asTempSv) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
            _asTempSv = asTempSv;
            _appSettings = options.Value;
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
        /// Genera el combo de ejercicios contables
        /// </summary>
        protected SelectList ComboEjercicios()
        {
            if (ViewBag.EjerciciosLista != null)
            {
                var lista = ViewBag.EjerciciosLista as List<EjercicioDto>;
                if (lista != null)
                {
                    return HelperMvc<ComboGenDto>.ListaGenerica(
                        lista.Select(e => new ComboGenDto
                        {
                            Id = e.Eje_nro,
                            Descripcion = e.Eje_lista
                        })
                    );
                }
            }
            return HelperMvc<ComboGenDto>.ListaGenerica(new List<ComboGenDto>());
        }

        /// <summary>
        /// Genera el combo de tipos de asiento
        /// </summary>
        protected SelectList ComboTiposAsiento()
        {
            if (ViewBag.TiposAsientoLista != null)
            {
                var lista = ViewBag.TiposAsientoLista as List<TipoAsientoDto>;
                if (lista != null)
                {
                    return HelperMvc<ComboGenDto>.ListaGenerica(
                        lista.Select(t => new ComboGenDto
                        {
                            Id = t.Dia_tipo,
                            Descripcion = t.Dia_lista
                        })
                    );
                }
            }
            return HelperMvc<ComboGenDto>.ListaGenerica(new List<ComboGenDto>());
        }

        /// <summary>
        /// Genera el combo de usuarios de ejercicio
        /// </summary>
        protected SelectList ComboUsuariosEjercicio()
        {
            if (UsuariosEjercicioLista != null && UsuariosEjercicioLista.Any())
            {
                return HelperMvc<ComboGenDto>.ListaGenerica(
                    UsuariosEjercicioLista.Select(u => new ComboGenDto
                    {
                        Id = u.Usu_id,
                        Descripcion = u.Usu_apellidoynombre
                    })
                );
            }
            return HelperMvc<ComboGenDto>.ListaGenerica(new List<ComboGenDto>());
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
