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
        private List<UsuAsientoDto> UsuariosEjercicioLista { get; set; } = [];

        public AsientoTemporalController(
            IOptions<AppSettings> options,
            IHttpContextAccessor contexto,
            ILogger<AsientoTemporalController> logger,
            IAsientoFrontServicio asientoServicio) : base(options, contexto, logger)
        {
            _asientoServicio = asientoServicio;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

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
            try
            {
                await ObtenerUsuariosDeEjercicio(eje_nro);
                var usuarios = UsuariosEjercicioLista.Select(u => new {
                    id = u.Usu_id,
                    text = u.Usu_apellidoynombre
                }).ToList();

                return Json(new { success = true, data = usuarios });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error al obtener usuarios del ejercicio {eje_nro}");
                return Json(new { success = false, message = "Ocurrió un error al obtener los usuarios del ejercicio" });
            }
        }
    }
}
