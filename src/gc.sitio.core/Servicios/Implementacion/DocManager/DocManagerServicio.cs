﻿using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.infraestructura.ViewModels;
using gc.sitio.core.Servicios.Contratos.DocManager;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion.DocManager
{
    public class DocManagerServicio : IDocManagerServicio
    {
        private const string RutaAPI = "/api/reportes";
        private const string RutaGenerar = "/generate";
        private const string RutaGenerarFormato = "/GenFileFormat";

        private readonly AppSettings _appSettings;
        private readonly DocsManager _docManager;
        private readonly ILogger<DocManagerServicio> _logger;

        public DocManagerServicio(IOptions<AppSettings> options, ILogger<DocManagerServicio> logger, IOptions<DocsManager> options1)
        {
            _appSettings = options.Value;
            _logger = logger;
            _docManager = options1.Value;
        }

        public List<MenuRootModal> GeneraArbolArchivos(AppModulo modulo)
        {
            List<Reporte> reportes = modulo.Reportes;
            List<MenuRootModal> arbol = new List<MenuRootModal>();
            MenuRootModal root = new MenuRootModal
            {
                id = modulo.Id,
                text = "Archivos",
                icon = "bx bx-file",
                state = new Estado { disabled = false, opened = true, selected = false},
                children = new List<MenuRootModal>()
            };

            foreach (var rep in reportes)
            {
                int contador = 0;
                foreach (var arch in rep.Titulos)
                {
                    var nodo = CargaArchivo(rep.Id, arch, contador);
                    // Asegurarnos de que el nodo tiene las propiedades correctas para mostrar el checkbox
                    if (nodo.state == null)
                    {
                        nodo.state = new Estado { disabled = false, opened = false, selected = false };
                    }
                    root.children.Add(nodo);
                    contador++;
                }
            }
            arbol.Add(root);
            return arbol;
        }

        private MenuRootModal CargaArchivo(int idm, string titulo, int orden)
        {
            //hay qeu tener en cuenta que si llegan a ser mas archivos en el mismo 
            //reporte, se deberia agregar un nuevo tipo de seleccion para el titulo del archivo.

            var id = idm.ToString(); // $"{idm}{orden + 1}";

            var archivo = new MenuRootModal
            {
                id = id,
                text = titulo,
                state = new Estado { disabled = false, opened = true, selected = false },
                data = new MenuRootData { archivoB64 = string.Empty, asignado = false }
            };

            return archivo;
        }

        /// <summary>
        /// Se devuelve la ruta del archivo generado
        /// </summary>
        /// <typeparam name="T">tipo de dato a utilizar</typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public void GenerarArchivoPDF<T>(PrintRequestDto<T> request, out MemoryStream ms, List<string> titulos, float[] anchos, bool datosCliente)
        {
            PdfWriter? writer = null;
            Document pdf;
            ms = new MemoryStream();
            try
            {
                var rCab = request.Cabecera;
                var rPie = request.Pie;

                string nnFile = rCab.TituloDocumento;

                #region Instancia del PDF y Generación de Cabecera
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, nnFile, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(rPie.Observaciones);

                var logo = HelperPdf.CargaLogo(rCab.Logo, 20, pdf.PageSize.Height - 10, 20);
                #region Se definen las fuentes a utilizar
                var titulo = HelperPdf.DefineFontWithStyle("Arial", 12, Font.BOLD, 0, 0, 0);
                var subtitulo = HelperPdf.DefineFontWithStyle("Arial", 10, Font.NORMAL, 0, 0, 0);
                var normal = HelperPdf.DefineFontWithStyle("Arial", 8, Font.NORMAL, 0, 0, 0);
                var chico = HelperPdf.DefineFontWithStyle("Arial", 6, Font.NORMAL, 0, 0, 0);
                #endregion
                #endregion

                #region Definición de la cabecera
                HeaderFooter cabecera = HelperPdf.GeneraCabeceraListadoTipo01(rCab, titulo, subtitulo, normal, chico, logo);
                pdf.Header = cabecera;
                #endregion
                #region Definición de Pie de página

                #endregion

                pdf.Open();

                #region Carga del listado
                //la primera hoja tiene los datos del cliente. Luego el listado de datos
                //cargamos los datos del cliente
                if (datosCliente)
                {
                    var tablaEnc = HelperPdf.GeneraTabla(4, [20f, 40f, 20f, 20f], 100, 10, 20);
                    HelperPdf.CargarDatosCliente(pdf, request.Cuerpo, subtitulo, tablaEnc);
                }

                //List<string> titulos = new List<string> { "Descripcion", "Cuota", "Est.", "Fecha Comp.", "Fecha Vto", "Importe" };
                //float[] anchos = [50f, 10f, 10f, 10f, 10f, 10f];
                HelperPdf.GeneraCabeceraLista(pdf, titulos, anchos, normal);

                HelperPdf.GenerarListadoDatos(pdf, request.Cuerpo, anchos, normal);


                #endregion
                //var parrafo = HelperPdf.GeneraParrafo("Texto Prueba", normal, Element.ALIGN_JUSTIFIED, 10, 10);
                //pdf.Add(parrafo);
                pdf.Close();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public DocumentManagerViewModel InicializaObjeto(string titulo, AppModulo _modulo)
        {
            return new DocumentManagerViewModel()
            {
                Id = _modulo.Id,
                Titulo = titulo,
                ShowExportOption = _modulo.Export,
                ShowPrintOption = _modulo.Print,
                ShowEmailOption = _modulo.Email,
                ShowWhatsAppOption = _modulo.Whatsapp,
            };
        }

        public List<MenuRoot> MarcarConsultaRealizada(List<MenuRoot> reportes, AppReportes consulta, int orden, string archB64, string tipoDato)
        {
            //debo buscar el reporte a modificar
            var id = $"{(int)consulta}{orden}";

            var repo = reportes[0].children.First(x => x.id == id);
            repo.state.selected = true;
            repo.state.disabled = false;
            repo.data.archivoB64 = archB64;
            repo.data.tipo = tipoDato;
            reportes[0].children.Remove(reportes[0].children.First(x => x.id == id));
            reportes[0].children.Add(repo);
            reportes[0].children = reportes[0].children.OrderBy(x => x.id).ToList();
            return reportes;
        }

        public async Task<RespuestaReportDto> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token)
        {
            ApiResponse<RespuestaReportDto> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(reporteSolicitud, token, out StringContent content);
                HttpResponseMessage response;
                var link = $"{_docManager.ApiReporteUrl}{RutaAPI}{RutaGenerar}";
                response = await client.PostAsync(link, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(stringData) ??
                            throw new NegocioException("Hubo un problema en la deserialización de los datos de la API.");
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con el reporte de Cuenta Corriente. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    string msg = "Hubo un error al intentar obtener el Informe de Cta Cte";
                    _logger.LogError($"{msg} {stringData}");
                    throw new NegocioException(msg);
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener el Informe de Cta Cte.");
                throw;
            }
        }

        public async Task<RespuestaReportDto> ObtenerRepoDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token)
        {
            ApiResponse<RespuestaReportDto> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(reporteSolicitud, token, out StringContent content);
                HttpResponseMessage response;
                var link = $"{_docManager.ApiReporteUrl}{RutaAPI}{RutaGenerarFormato}";
                response = await client.PostAsync(link, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(stringData) ??
                            throw new NegocioException("Hubo un problema en la deserialización de los datos de la API.");
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con el reporte de Cuenta Corriente. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    string msg = "Hubo un error al intentar obtener el Informe de Cta Cte";
                    _logger.LogError($"{msg} {stringData}");
                    throw new NegocioException(msg);
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener el Informe de Cta Cte.");
                throw;
            }
        }
    }
}
