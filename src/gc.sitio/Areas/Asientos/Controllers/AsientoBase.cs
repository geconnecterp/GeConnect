using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Asientos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Asientos.Controllers
{
    public class AsientoBase:ControladorBase
    {

        protected List<UsuAsientoDto> UsuariosEjercicioLista { get; set; } = [];


        public AsientoBase(IOptions<AppSettings> options,IHttpContextAccessor contexto,ILogger logger ):base(options,contexto,logger)
        {
            
        }

        /// <summary>
        /// Obtiene los ejercicios contables para el combo
        /// </summary>
        protected async Task ObtenerEjerciciosContables(IAsientoFrontServicio _asientoServicio)
        {
            try
            {
                var response = await _asientoServicio.ObtenerEjercicios(TokenCookie);
                if (response.Ok && response.ListaEntidad != null)
                {
                    ViewBag.EjerciciosLista = response.ListaEntidad.OrderByDescending(x => x.Eje_desde).ToList();
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
        protected async Task ObtenerTiposAsiento(IAsientoFrontServicio _asientoServicio)
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
        protected async Task ObtenerUsuariosDeEjercicio(int eje_nro, IAsientoFrontServicio _asientoServicio)
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

    }
}
