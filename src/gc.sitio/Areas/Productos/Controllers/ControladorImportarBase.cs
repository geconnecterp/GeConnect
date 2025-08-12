using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Importacion;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorImportarBase : ControladorBase
    {
        public ControladorImportarBase(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger logger) : base(options, contexto, logger)
        {

        }

        public List<PrecioFileDatos> DatosParaImportacion
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("DatosParaImportacion") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new List<PrecioFileDatos>();
                }
                return JsonConvert.DeserializeObject<List<PrecioFileDatos>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("DatosParaImportacion", json);
            }
        }


        protected async Task ObtenerDatosParaImportacion(IImportarServicio _impServicio)
        {
            // Cargar datos iniciales para la importación
            var datos = await _impServicio.ObtenerPrecioFileDatos(TokenCookie);
            DatosParaImportacion = datos.ListaEntidad ?? [];


        }
    }
}