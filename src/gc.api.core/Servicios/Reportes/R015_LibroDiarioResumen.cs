using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Reportes
{
    public class R015_LibroDiarioResumen : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiLDiarioServicio _lDiarioSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R015_LibroDiarioResumen(IUnitOfWork uow,
            IApiLDiarioServicio ldSv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _lDiarioSv = ldSv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Fecha", "N° Movimiento", "Concepto", "Debe", "Haber", "Saldo" };
            _campos = new List<string> { "Fecha", "Movimiento", "Concepto", "Debe", "Haber", "Saldo" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {

            float[] anchos;
            string ctaId = "";
            PdfWriter? writer = null;
            Document pdf;
            try
            {
                var ms = new MemoryStream();

                #region Obteniendo registros desde la base de datos

                var registros = ObtenerDatos(solicitud, out ctaId);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron Asientos. Deberia reformular los parámetros de consulta.");
                }

                
                #endregion
                #region Scripts PDF

                anchos = [60f, 250f, 100f, 100f];
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);
                Image? logo;
                if (!string.IsNullOrEmpty(solicitud.LogoPath))
                {
                    logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);
                }
                else
                {
                    logo = null;
                }

                #endregion

                #region Generación de Cabecera               

                PdfPTable tabla = GeneraCabeceraPDF2(solicitud, chico, titulo, logo, _empresaGeco);

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
                Paragraph subText;
                subText = HelperPdf.GeneraParrafo(solicitud.SubTitulo, subtitulo, Element.ALIGN_LEFT, 3, 3);
                pdf.Add(subText);

                //HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                #region Carga del cuerpo del reporte

                if (!string.IsNullOrEmpty(solicitud.Titulo))
                {
                    Paragraph leyenda = HelperPdf.GeneraParrafo(solicitud.Titulo, normalBold, Element.ALIGN_LEFT, 0, 5);
                    pdf.Add(leyenda);
                }

                // AQUÍ AGREGAMOS EL CUERPO DEL REPORTE
                CargaCuerpoReporte(pdf, registros, normal, normalBold, chico);
                #endregion

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
                _logger.LogError(ex, "Error en R001");

                throw new NegocioException($"Se produjo un error al intentar generar la Impresión del Asiento {ctaId}. Para mayores datos ver el log.");
            }

        }


        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }

        private List<LibroDiarioResumen> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
        {

            var eje_nro = solicitud.Parametros.GetValueOrDefault("eje_nro", "");
            var movimientos = solicitud.Parametros.GetValueOrDefault("movimientos", "");
            var ConTemporales = solicitud.Parametros.GetValueOrDefault("conTemporales", "").ToBoolean();
            var rango = solicitud.Parametros.GetValueOrDefault("rango", "").ToBoolean();
            var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
            var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
            var hasCarga = solicitud.Parametros.GetValueOrDefault("rangoFC", "").ToBoolean();
            var cDesde = solicitud.Parametros.GetValueOrDefault("desdeFC", "").ToDateTime();
            var cHasta = solicitud.Parametros.GetValueOrDefault("hastaFC", "").ToDateTime();



            //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
            //string dia_movi = solicitud.Parametros.GetValueOrDefault("dia_movi", "") ?? "";
            id = eje_nro;
            return _lDiarioSv.ObtenerAsientoLibroDiarioResumen(eje_nro.ToInt(),rango, desde, hasta,
                hasCarga, cDesde, cHasta,
                movimientos, ConTemporales, 99999, 1, "");
        }

        private void CargaCuerpoReporte(Document pdf, List<LibroDiarioResumen> datos, Font normal, Font normalBold, Font chico)
        {
            if (datos == null || !datos.Any())
            {
                Paragraph noData = new Paragraph("No se encontraron datos para mostrar.", normalBold);
                noData.Alignment = Element.ALIGN_CENTER;
                pdf.Add(noData);
                return;
            }

            // Variables para control de agrupamiento
            string currentTipo = string.Empty;
            decimal totalDebe = 0;
            decimal totalHaber = 0;
            decimal totalGeneralDebe = 0;
            decimal totalGeneralHaber = 0;

            // Definir anchos de columnas
            float[] anchosColumnas = new float[] { 60f, 250f, 100f, 100f };

            PdfPTable tabla = null;

            // Proceso principal - agrupar por dia_tipo
            foreach (var item in datos.OrderBy(d => d.Dia_tipo))
            {
                // Verificar si cambia el tipo de asiento
                if (item.Dia_tipo != currentTipo)
                {
                    // Si hay un tipo anterior, mostrar totales
                    if (!string.IsNullOrEmpty(currentTipo) && tabla != null)
                    {
                        // Agregar fila de totales para el grupo anterior
                        AgregarFilaTotales(tabla, totalDebe, totalHaber, normalBold);

                        // Añadir la tabla al PDF
                        pdf.Add(tabla);

                        // Agregar espacio entre grupos
                        pdf.Add(Chunk.Newline);
                    }

                    // Actualizar el tipo actual y reiniciar totales del grupo
                    currentTipo = item.Dia_tipo;
                    totalDebe = 0;
                    totalHaber = 0;

                    // Agregar encabezado para el nuevo grupo
                    Paragraph tipoHeader = new Paragraph($"Tipo de Asiento: {item.Dia_tipo}", normalBold);
                    tipoHeader.SpacingBefore = 10f;
                    tipoHeader.SpacingAfter = 5f;
                    pdf.Add(tipoHeader);

                    // Crear nueva tabla para este grupo
                    tabla = new PdfPTable(4);
                    tabla.WidthPercentage = 100;
                    tabla.SetWidths(anchosColumnas);

                    // Agregar encabezados de columnas
                    AgregarEncabezadosColumnas(tabla, normalBold);
                }

                // Agregar fila de datos
                AgregarFilaDatos(tabla, item, normal, chico);

                // Actualizar totales
                totalDebe += item.dia_debe;
                totalHaber += item.dia_haber;
                totalGeneralDebe += item.dia_debe;
                totalGeneralHaber += item.dia_haber;
            }

            // Agregar el último grupo si hay datos
            if (tabla != null)
            {
                // Agregar totales del último grupo
                AgregarFilaTotales(tabla, totalDebe, totalHaber, normalBold);
                pdf.Add(tabla);
            }

            // Agregar total general
            pdf.Add(Chunk.Newline);
            PdfPTable tablaTotal = new PdfPTable(4);
            tablaTotal.WidthPercentage = 100;
            tablaTotal.SetWidths(anchosColumnas);

            // Celda vacía
            PdfPCell cellVacia = new PdfPCell(new Phrase("", normal));
            cellVacia.Border = Rectangle.NO_BORDER;
            tablaTotal.AddCell(cellVacia);

            // Celda "Total General:"
            PdfPCell cellTotalGeneral = new PdfPCell(new Phrase("Total General:", normalBold));
            cellTotalGeneral.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotalGeneral.Border = Rectangle.NO_BORDER;
            tablaTotal.AddCell(cellTotalGeneral);

            // Total Debe
            PdfPCell cellTotalDebe = new PdfPCell(new Phrase(totalGeneralDebe.ToString("N2"), normalBold));
            cellTotalDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotalDebe.Border = Rectangle.NO_BORDER;
            tablaTotal.AddCell(cellTotalDebe);

            // Total Haber
            PdfPCell cellTotalHaber = new PdfPCell(new Phrase(totalGeneralHaber.ToString("N2"), normalBold));
            cellTotalHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotalHaber.Border = Rectangle.NO_BORDER;
            tablaTotal.AddCell(cellTotalHaber);

            pdf.Add(tablaTotal);
        }

        // Método para agregar los encabezados de columnas
        private void AgregarEncabezadosColumnas(PdfPTable tabla, Font normalBold)
        {
            PdfPCell cellCuenta = new PdfPCell(new Phrase("Cuenta", normalBold));
            cellCuenta.HorizontalAlignment = Element.ALIGN_CENTER;
            cellCuenta.BackgroundColor = BaseColor.LightGray;
            tabla.AddCell(cellCuenta);

            PdfPCell cellDesc = new PdfPCell(new Phrase("Descripción Cuenta", normalBold));
            cellDesc.HorizontalAlignment = Element.ALIGN_CENTER;
            cellDesc.BackgroundColor = BaseColor.LightGray;
            tabla.AddCell(cellDesc);

            PdfPCell cellDebe = new PdfPCell(new Phrase("Debe", normalBold));
            cellDebe.HorizontalAlignment = Element.ALIGN_CENTER;
            cellDebe.BackgroundColor = BaseColor.LightGray;
            tabla.AddCell(cellDebe);

            PdfPCell cellHaber = new PdfPCell(new Phrase("Haber", normalBold));
            cellHaber.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHaber.BackgroundColor = BaseColor.LightGray;
            tabla.AddCell(cellHaber);
        }

        // Método para agregar una fila de datos
        private void AgregarFilaDatos(PdfPTable tabla, LibroDiarioResumen item, Font normal, Font chico)
        {
            // Celda para la cuenta
            PdfPCell cellCuenta;
            if (item.temporal == true)
            {
                // Para asientos temporales, agregar la "X" al inicio
                Paragraph cuentaParrafo = new Paragraph();
                Font redFont = new Font(normal);
                redFont.Color = BaseColor.Red;
                cuentaParrafo.Add(new Chunk("X ", redFont));
                cuentaParrafo.Add(new Chunk(item.Ccb_id, normal));
                cellCuenta = new PdfPCell(cuentaParrafo);
            }
            else
            {
                cellCuenta = new PdfPCell(new Phrase(item.Ccb_id, normal));
            }
            cellCuenta.HorizontalAlignment = Element.ALIGN_LEFT;
            tabla.AddCell(cellCuenta);

            // Descripción de la cuenta
            PdfPCell cellDesc = new PdfPCell(new Phrase(item.Ccb_desc, normal));
            cellDesc.HorizontalAlignment = Element.ALIGN_LEFT;
            tabla.AddCell(cellDesc);

            // Debe
            PdfPCell cellDebe = new PdfPCell(new Phrase(item.dia_debe > 0 ? item.dia_debe.ToString("N2") : "0.00", normal));
            cellDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
            tabla.AddCell(cellDebe);

            // Haber
            PdfPCell cellHaber = new PdfPCell(new Phrase(item.dia_haber > 0 ? item.dia_haber.ToString("N2") : "0.00", normal));
            cellHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
            tabla.AddCell(cellHaber);
        }

        // Método para agregar una fila de totales
        private void AgregarFilaTotales(PdfPTable tabla, decimal totalDebe, decimal totalHaber, Font normalBold)
        {
            // Celda vacía
            PdfPCell cellVacia = new PdfPCell(new Phrase("", normalBold));
            cellVacia.Border = Rectangle.TOP_BORDER;
            tabla.AddCell(cellVacia);

            // Celda "Total:"
            PdfPCell cellTotal = new PdfPCell(new Phrase("Total:", normalBold));
            cellTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotal.Border = Rectangle.TOP_BORDER;
            tabla.AddCell(cellTotal);

            // Total Debe
            PdfPCell cellTotalDebe = new PdfPCell(new Phrase(totalDebe.ToString("N2"), normalBold));
            cellTotalDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotalDebe.Border = Rectangle.TOP_BORDER;
            tabla.AddCell(cellTotalDebe);

            // Total Haber
            PdfPCell cellTotalHaber = new PdfPCell(new Phrase(totalHaber.ToString("N2"), normalBold));
            cellTotalHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellTotalHaber.Border = Rectangle.TOP_BORDER;
            tabla.AddCell(cellTotalHaber);
        }

    }
}
