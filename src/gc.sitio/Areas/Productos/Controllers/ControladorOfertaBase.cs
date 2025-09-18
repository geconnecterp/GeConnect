using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorOfertaBase:ControladorBase
    {
        public ControladorOfertaBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger)
            :base(options,contexto,logger)
        {
            
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


        internal void CargarDatosIniciales(bool actualizar, ICuentaServicio _cuentaServicio,IRubroServicio _rubroServicio)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
        }

    }
}
