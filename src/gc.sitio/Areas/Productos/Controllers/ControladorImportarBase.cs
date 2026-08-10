using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
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

        /// <summary>
        /// Se Obtenen los datos de referencia para la importación de listas de precios.
        /// sera el combo que aparece en la vista de importación
        /// </summary>
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

        /// <summary>
        /// metodo para obtener los datos de referencia para la importación de listas de precios.
        /// </summary>
        /// <param name="_impServicio"></param>
        /// <returns></returns>
        protected async Task ObtenerDatosParaImportacion(IImportarServicio _impServicio)
        {
            // Cargar datos iniciales para la importación
            var datos = await _impServicio.ObtenerPrecioFileDatos(TokenCookie);
            DatosParaImportacion = datos.ListaEntidad ?? [];
        }

        protected async Task ObtenerPerfilDeProveedor(IImportarServicio _impServicio, string ctaId)
        {
            var datos = await _impServicio.ObtenerPerfilDeProveedor(ctaId, TokenCookie);
            PerfilProveedorGuardado = datos.Ok ? datos.ListaEntidad ?? [] : [];
        }

        /// <summary>
        /// Perfil persistido del proveedor. Se conserva separado del análisis físico
        /// del archivo para poder aplicarlo y compararlo sin sobrescribirlo.
        /// </summary>
        public List<MapeoColumnaDto> PerfilProveedorGuardado
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("PerfilProveedorGuardado") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }

                return JsonConvert.DeserializeObject<List<MapeoColumnaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value ?? []);
                _context.HttpContext?.Session.SetString("PerfilProveedorGuardado", json);
            }
        }

        /// <summary>
        /// Variable para almacenar el análisis del archivo Excel. El mismo se carga
        /// con el perfil del proveedor seleccionado en la vista de importación.
        /// o se carga con el análisis del archivo Excel que se sube en la vista de importación.
        /// </summary>
        public AnalisisExcelDto AnalisisFile {
            get
            {
                var json = _context.HttpContext?.Session.GetString("AnalisisFile") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new AnalisisExcelDto();
                }
                return JsonConvert.DeserializeObject<AnalisisExcelDto>(json) ?? new();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("AnalisisFile", json);
            }
        }


    }
}
