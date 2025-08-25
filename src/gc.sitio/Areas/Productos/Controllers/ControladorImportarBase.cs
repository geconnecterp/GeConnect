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

        protected void ObtenerPerfilDeProveedor(IImportarServicio _impServicio, string ctaId)
        {
            // Cargar datos iniciales para la importación
            RespuestaGenerica<MapeoColumnaDto> datos = _impServicio.ObtenerPerfilDeProveedor(ctaId, TokenCookie).GetAwaiter().GetResult();

           MappeoPerfil2Analisis(datos);
            
        }

        private void MappeoPerfil2Analisis(RespuestaGenerica<MapeoColumnaDto> datos)
        {
            if (datos == null || !datos.Ok || (datos != null && datos.Ok && datos.ListaEntidad?.Count == 0))
            {
                AnalisisFile = new();
            }

            // ✅ PASO 1: Crear análisis base (sin datos específicos ya que vienen del mapeo guardado)
            AnalisisExcelDto analisisExcelDto = new AnalisisExcelDto
            {
                TotalColumnas = datos.ListaEntidad.Count,
                TotalColumnasUtiles = datos.ListaEntidad.Count(m => !string.IsNullOrEmpty(m.campo_bd)),
                CamposDisponibles = DatosParaImportacion // Usar los datos ya cargados
            };

            // ✅ PASO 2: Convertir mapeos guardados a columnas de análisis
            analisisExcelDto.Columnas = [];
            var ctaId = ProveedorSeleccionado.Cta_Id;
            foreach (var mapeo in datos?.ListaEntidad ?? [])
            {
                ColumnaExcelDto columna = new ColumnaExcelDto
                {
                    Indice = mapeo.indice_columna,
                    Letra = mapeo.letra_columna,
                    Encabezado = mapeo.encabezado_original,
                    TipoDetectado = mapeo.tipo_dato,
                    CampoMapeado = mapeo.campo_bd,
                    DescripcionMapeado = mapeo.descripcion_campo,
                    ConfianzaMapeo = mapeo.confianza_mapeo,
                    MapeadoAutomatico = mapeo.mapeado_automatico
                };

                analisisExcelDto.Columnas.Add(columna);
            }

            AnalisisFile = analisisExcelDto;
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