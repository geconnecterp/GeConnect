using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Reportes
{
    public class R009_InformeCuentaCorriente : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R009_InformeCuentaCorriente(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Dia Movi", "Descripcion" };
            _campos = new List<string> { "resultado_id", "resultado_msj" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {

            float[] anchos;

            PdfWriter? writer = null;
            Document pdf;
            try
            {
                var ms = new MemoryStream();

                #region Obteniendo registros desde la base de datos

                List<RespuestaDto> regErr = ObtenerDatos(solicitud, out string ctaId);

                if (regErr == null || regErr.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }

                #endregion               
                #region Scripts PDF

                anchos = [30f, 70f];
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                Image? logo = null;
                if (solicitud.LogoPath != null)
                {
                    logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);
                }

                #endregion

                #region Generación de Cabecera               

                PdfPTable tabla = HelperPdf.GeneraTabla(4, [10f, 20f, 50f, 20f], 100, 10, 20);

                // Columna 1: Logo
                PdfPCell celdaLogo;
                if (logo != null)
                {
                    celdaLogo = HelperPdf.GeneraCelda(logo, false);
                }
                else
                {
                    celdaLogo = new PdfPCell(new Paragraph(""));

                }
                tabla.AddCell(celdaLogo);

                // Columna 2: Datos apilados y título

                tabla.AddCell(new Paragraph(""));
                // Columna 3: Título del informe
                PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 10f
                };
                tabla.AddCell(celdaTitulo);

                // Columna 4: Fecha
                string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                tabla.AddCell(celdaFechaHora);

                // Convertir la tabla en un Phrase
                Phrase phrase = new Phrase();
                phrase.Add(tabla);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                    //BorderWidthBottom = 1,   

                };

                pdf.Header = header;
                #endregion

                pdf.Open();

                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);
                HelperPdf.GenerarListadoDesdeLista(pdf, regErr, _campos, anchos, chico);

                #endregion

                pdf.Close();
                #endregion

                return Convert.ToBase64String(ms.ToArray());

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //_logger.Log(typeof(R001_InformeCuentaCorriente), Level.Error, $"Error al generar el informe de cuenta corriente: {ex.Message}", ex);
                _logger.LogError(ex, "Error en R009");

                throw new NegocioException("Se produjo un error al intentar generar el Informe de Errores al confirmar a la Contabilidad Definitiva. Para mayores datos ver el log.");
            }

        }


        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<RespuestaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            #endregion

            return GeneraTXT(registros, _campos);
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<RespuestaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            #endregion

            return GeneraFileXLS(registros, _titulos, _campos);
        }



        private List<RespuestaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId)
        {

            //Se obtienen los parámetros del reporte
            //Se obtiene la cuenta del cliente (ctaId) y la fecha desde (fechaD)
            ctaId = "";
            DateTime fecha;
            var fechaStr = solicitud.Parametros.GetValueOrDefault("fecha", "").ToString();

            if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out fecha))
            {
                throw new NegocioException("La fecha desde es inválida.");
            }

            fecha = fechaStr.ToDateTime();

            //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
            string jsonErr = solicitud.Parametros.GetValueOrDefault("errores", "").ToString() ?? "";
            var resultados = JsonConvert.DeserializeObject<List<RespuestaDto>>(jsonErr);

            return resultados;
        }


    }
}
