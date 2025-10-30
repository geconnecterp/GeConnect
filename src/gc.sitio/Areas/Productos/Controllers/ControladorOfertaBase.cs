using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorOfertaBase:ControladorBase
    {
        public ControladorOfertaBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger)
            :base(options,contexto,logger)
        {
            
        }

        public List<OfertaDto> OfertasSinActivar
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("OfertasSinActivar") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<OfertaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("OfertasSinActivar", json);
            }
        }

        public List<OfertaDto> OfertasActivas
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("OfertasActivas") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<OfertaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("OfertasActivas", json);
            }
        }

        public List<ComboSustitutoDto> ProductosSustitutos
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ProductosSustitutos") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ComboSustitutoDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosSustitutos", json);
            }
        }

        /// <summary>
        /// Crea una respuesta de error estandarizada
        /// </summary>
        internal RespuestaGenerica<EntidadBase> CrearRespuestaError(string mensaje)
        {
            return new RespuestaGenerica<EntidadBase>
            {
                Mensaje = mensaje,
                Ok = false,
                EsWarn = false,
                EsError = true
            };
        }

        /// <summary>
        /// Crea una respuesta de Warning estandarizada
        /// </summary>
        internal RespuestaGenerica<EntidadBase> CrearRespuestaWarning(string mensaje)
        {
            return new RespuestaGenerica<EntidadBase>
            {
                Mensaje = mensaje,
                Ok = false,
                EsWarn = true,
                EsError = false
            };
        }


        internal void CargarDatosIniciales(bool actualizar, ICuentaServicio _cuentaServicio,IRubroServicio _rubroServicio,IComboServicio _comboServicio = null)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            if (_comboServicio != null)
            {
                if (ComboTipoLista.Count == 0 || actualizar)
                {

                    var resTipo = _comboServicio.ObtenerComboTipos(TokenCookie).GetAwaiter().GetResult();
                    if (!resTipo.Ok)
                        throw new NegocioException(resTipo.Mensaje ?? "Hubo un problema para obtener los tipos");
                    ComboTipoLista = resTipo.ListaEntidad ?? [];
                }

                if(ComboEstadoLista.Count==0 || actualizar)
                {
                    var resEstado = _comboServicio.ObtenerComboEstados(TokenCookie).GetAwaiter().GetResult();
                    if (!resEstado.Ok)
                        throw new NegocioException(resEstado.Mensaje ?? "Hubo un problema para obtener los estados");
                    ComboEstadoLista = resEstado.ListaEntidad ?? [];
                }                
            }

            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
        }


        internal SelectList ComboPresupuestoEstado(string sel = "")
        {
            var est = EstadosPresupuesto;
            var estCbo = est.Select(x => new ComboGenDto { Id = x.pree_id.ToString(), Descripcion = x.pree_desc }).ToList();
            if (string.IsNullOrEmpty(sel))
            {
                return HelperMvc<ComboGenDto>.ListaGenerica(estCbo);
            }
            return HelperMvc<ComboGenDto>.ListaGenerica(estCbo, sel);
        }

        internal SelectList ComboPresupuestoTipo(string sel = "")
        {
            var tipo = TiposPresupuesto;
            var tipoCbo = tipo.Select(x => new ComboGenDto { Id = x.pret_id.ToString(), Descripcion = x.pret_desc }).ToList();
            if (string.IsNullOrEmpty(sel))
            {
                return HelperMvc<ComboGenDto>.ListaGenerica(tipoCbo);
            }
            return HelperMvc<ComboGenDto>.ListaGenerica(tipoCbo, sel);
        }

    }
}
