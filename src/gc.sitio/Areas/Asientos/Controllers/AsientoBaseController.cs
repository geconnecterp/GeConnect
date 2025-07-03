using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Asientos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Asientos.Controllers
{
    public class AsientoBaseController:ControladorBase
    {
       
        protected List<UsuAsientoDto> UsuariosEjercicioLista { get; set; } = [];
        

        public AsientoBaseController(IOptions<AppSettings> options,IHttpContextAccessor contexto,ILogger logger ):base(options,contexto,logger)
        {
            
        }

        public List<AsientoAjusteDto> AsientosAjuste
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("AsientosAjuste") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<AsientoAjusteDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("AsientosAjuste", json);
            }
        }

        public List<AsientoAjusteCcbDto> AsientosAjusteCcb
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("AsientosAjusteCcb") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<AsientoAjusteCcbDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("AsientosAjusteCcb", json);
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


        protected List<AsientoPlanoDto> ConvertirAsientoAPlano(AsientoDetalleDto asiento)
        {

            var asientosPlanos = new List<AsientoPlanoDto>();

            foreach (var linea in asiento.Detalles)
            {
                var asientoPlano = new AsientoPlanoDto
                {
                    eje_nro = asiento.eje_nro,
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
    }
}
