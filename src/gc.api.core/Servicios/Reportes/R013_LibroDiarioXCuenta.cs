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
    public class R013_LibroDiarioXCuenta : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiLDiarioServicio _lDiarioSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R013_LibroDiarioXCuenta(IUnitOfWork uow,
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

                anchos = [12f, 30f, 30f, 14f, 14f];
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

                // Definir colores para mejorar la legibilidad del reporte
                BaseColor colorCabecera = new BaseColor(245, 245, 245); // Gris claro para encabezados
                BaseColor colorSaldo = new BaseColor(240, 240, 240);     // Gris para saldos

                // Inicializar totales generales
                decimal totalGeneralDebe = 0;
                decimal totalGeneralHaber = 0;

                // Iterar sobre cada asiento contable
                foreach (var asiento in registros)
                {
                    // 1. CREAR CABECERA DEL ASIENTO
                    PdfPTable tablaCabecera = new PdfPTable(2);
                    tablaCabecera.WidthPercentage = 100;
                    tablaCabecera.SetWidths(new float[] { 60f, 40f });
                    tablaCabecera.SpacingBefore = 10f;

                    // Columna izquierda: Fecha y Descripción
                    var celdaFecha = new PdfPCell(new Phrase($"Fecha de Asiento: {asiento.Dia_fecha:dd/MM/yyyy}", normalBold));
                    celdaFecha.BackgroundColor = colorCabecera;
                    celdaFecha.Border = Rectangle.NO_BORDER;
                    celdaFecha.HorizontalAlignment = Element.ALIGN_LEFT;
                    celdaFecha.Padding = 5;
                    tablaCabecera.AddCell(celdaFecha);

                    // Columna derecha: Número de Movimiento
                    var celdaMovimiento = new PdfPCell(new Phrase($"Movimiento Nº: {asiento.Dia_movi}", normalBold));
                    celdaMovimiento.BackgroundColor = colorCabecera;
                    celdaMovimiento.Border = Rectangle.NO_BORDER;
                    celdaMovimiento.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaMovimiento.Padding = 5;
                    tablaCabecera.AddCell(celdaMovimiento);

                    pdf.Add(tablaCabecera);

                    // Descripción del asiento
                    PdfPTable tablaDescripcion = new PdfPTable(1);
                    tablaDescripcion.WidthPercentage = 100;

                    var celdaDescripcion = new PdfPCell(new Phrase($"Descripción Asiento: {asiento.Dia_desc_asiento}", normal));
                    celdaDescripcion.BackgroundColor = colorCabecera;
                    celdaDescripcion.Border = Rectangle.NO_BORDER;
                    celdaDescripcion.HorizontalAlignment = Element.ALIGN_LEFT;
                    celdaDescripcion.Padding = 5;
                    tablaDescripcion.AddCell(celdaDescripcion);

                    pdf.Add(tablaDescripcion);

                    // 2. CREAR TABLA DE DETALLE DEL ASIENTO
                    PdfPTable tablaDetalle = new PdfPTable(5);
                    tablaDetalle.WidthPercentage = 100;
                    tablaDetalle.SetWidths(new float[] { 10f, 25f, 35f, 15f, 15f });

                    // Cabeceras de la tabla
                    string[] cabeceras = { "Cuenta", "Descripción Cuenta", "Concepto", "Debe", "Haber" };
                    foreach (var tit in cabeceras)
                    {
                        var celda = new PdfPCell(new Phrase(tit, normalBold));
                        celda.BackgroundColor = new BaseColor(230, 230, 230);
                        celda.HorizontalAlignment = (tit == "Debe" || tit == "Haber") ?
                            Element.ALIGN_RIGHT : Element.ALIGN_LEFT;
                        celda.Padding = 5;
                        tablaDetalle.AddCell(celda);
                    }

                    // Variables para los totales del asiento actual
                    decimal totalDebe = 0;
                    decimal totalHaber = 0;
                    bool filaAlterna = false;

                    // Filas de detalle del asiento
                    if (asiento.Detalles != null && asiento.Detalles.Any())
                    {
                        foreach (var detalle in asiento.Detalles)
                        {
                            // Actualizar totales
                            totalDebe += detalle.Debe;
                            totalHaber += detalle.Haber;
                            totalGeneralDebe += detalle.Debe;
                            totalGeneralHaber += detalle.Haber;

                            // Color de fondo alterno para mejorar legibilidad
                            BaseColor colorFondo = filaAlterna ? new BaseColor(249, 249, 249) : BaseColor.White;
                            filaAlterna = !filaAlterna;

                            // Columna Cuenta
                            var celdaCuenta = new PdfPCell(new Phrase(detalle.Ccb_id, normal));
                            celdaCuenta.BackgroundColor = colorFondo;
                            celdaCuenta.HorizontalAlignment = Element.ALIGN_LEFT;
                            celdaCuenta.Padding = 5;
                            tablaDetalle.AddCell(celdaCuenta);

                            // Columna Descripción Cuenta
                            var celdaDescCuenta = new PdfPCell(new Phrase(detalle.Ccb_desc, normal));
                            celdaDescCuenta.BackgroundColor = colorFondo;
                            celdaDescCuenta.HorizontalAlignment = Element.ALIGN_LEFT;
                            celdaDescCuenta.Padding = 5;
                            tablaDetalle.AddCell(celdaDescCuenta);

                            // Columna Concepto
                            var celdaConcepto = new PdfPCell(new Phrase(detalle.Dia_desc, normal));
                            celdaConcepto.BackgroundColor = colorFondo;
                            celdaConcepto.HorizontalAlignment = Element.ALIGN_LEFT;
                            celdaConcepto.Padding = 5;
                            tablaDetalle.AddCell(celdaConcepto);

                            // Columna Debe
                            var celdaDebe = new PdfPCell(new Phrase(detalle.Debe > 0 ? detalle.Debe.ToString("N2") : "0.00", normal));
                            celdaDebe.BackgroundColor = colorFondo;
                            celdaDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
                            celdaDebe.Padding = 5;
                            tablaDetalle.AddCell(celdaDebe);

                            // Columna Haber
                            var celdaHaber = new PdfPCell(new Phrase(detalle.Haber > 0 ? detalle.Haber.ToString("N2") : "0.00", normal));
                            celdaHaber.BackgroundColor = colorFondo;
                            celdaHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
                            celdaHaber.Padding = 5;
                            tablaDetalle.AddCell(celdaHaber);
                        }
                    }

                    // 3. FILA DE SALDO DEL ASIENTO
                    var celdaSaldoDesc = new PdfPCell(new Phrase("Subtotal:", normalBold));
                    celdaSaldoDesc.Colspan = 3;
                    celdaSaldoDesc.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaSaldoDesc.BackgroundColor = colorSaldo;
                    celdaSaldoDesc.Padding = 5;
                    tablaDetalle.AddCell(celdaSaldoDesc);

                    var celdaSaldoDebe = new PdfPCell(new Phrase(totalDebe.ToString("N2"), normalBold));
                    celdaSaldoDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaSaldoDebe.BackgroundColor = colorSaldo;
                    celdaSaldoDebe.Padding = 5;
                    tablaDetalle.AddCell(celdaSaldoDebe);

                    var celdaSaldoHaber = new PdfPCell(new Phrase(totalHaber.ToString("N2"), normalBold));
                    celdaSaldoHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaSaldoHaber.BackgroundColor = colorSaldo;
                    celdaSaldoHaber.Padding = 5;
                    tablaDetalle.AddCell(celdaSaldoHaber);

                    // Agregar la tabla de detalle al documento
                    pdf.Add(tablaDetalle);
                }

                // 4. TOTALES GENERALES AL FINAL DEL DOCUMENTO
                if (registros.Any())
                {
                    PdfPTable tablaTotal = new PdfPTable(3);
                    tablaTotal.WidthPercentage = 100;
                    tablaTotal.SetWidths(new float[] { 70f, 15f, 15f });
                    tablaTotal.SpacingBefore = 10f;

                    // Celda de descripción de saldo
                    var celdaTotalDesc = new PdfPCell(new Phrase("Total Final:", normalBold));
                    celdaTotalDesc.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaTotalDesc.BackgroundColor = new BaseColor(220, 220, 220);
                    celdaTotalDesc.Padding = 6;
                    tablaTotal.AddCell(celdaTotalDesc);

                    // Celda de total debe
                    var celdaTotalDebe = new PdfPCell(new Phrase(totalGeneralDebe.ToString("N2"), normalBold));
                    celdaTotalDebe.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaTotalDebe.BackgroundColor = new BaseColor(220, 220, 220);
                    celdaTotalDebe.Padding = 6;
                    tablaTotal.AddCell(celdaTotalDebe);

                    // Celda de total haber
                    var celdaTotalHaber = new PdfPCell(new Phrase(totalGeneralHaber.ToString("N2"), normalBold));
                    celdaTotalHaber.HorizontalAlignment = Element.ALIGN_RIGHT;
                    celdaTotalHaber.BackgroundColor = new BaseColor(220, 220, 220);
                    celdaTotalHaber.Padding = 6;
                    tablaTotal.AddCell(celdaTotalHaber);

                    pdf.Add(tablaTotal);
                }

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

        private List<AsientoDetalleLDDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
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
            return _lDiarioSv.ObtenerAsientoLibroDiario(eje_nro.ToInt(),rango, desde, hasta,
                hasCarga, cDesde, cHasta,
                movimientos, ConTemporales, 99999, 1, "");
        }
    }
}
