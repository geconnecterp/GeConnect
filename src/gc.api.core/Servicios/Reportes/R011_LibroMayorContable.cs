using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R011_LibroMayorContable : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiLMayorServicio _lMayorSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R011_LibroMayorContable(IUnitOfWork uow, IApiLMayorServicio lmSv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _lMayorSv = lmSv;

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

                var regs = registros.Select(x => new
                {
                    Fecha = x.Dia_fecha.ToShortDateString(),
                    Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                    Concepto = x.Dia_desc,
                    Debe = x.Dia_debe,
                    Haber = x.Dia_haber,
                    Saldo = x.Dia_saldo
                }).ToList();

                #endregion
                #region Scripts PDF

                anchos = [12f, 12f, 40f, 12f, 12f, 12f];
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
                subText = HelperPdf.GeneraParrafo(solicitud.SubTitulo, titulo, Element.ALIGN_LEFT, 3, 3);
                pdf.Add(subText);

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                #region Carga del cuerpo
               
                // Variables para agrupar por fecha y calcular subtotales
                DateTime? fechaActual = null;
                decimal subTotalDebe = 0;
                decimal subTotalHaber = 0;
                decimal totalDebe = 0;
                decimal totalHaber = 0;

                // Agregar fila de saldo anterior
                decimal saldoInicial = registros.First().Saldo_inicial;
                PdfPTable tablaSaldoAnterior = new PdfPTable(6);
                tablaSaldoAnterior.SetWidths(anchos);
                tablaSaldoAnterior.WidthPercentage = 100;

                // Creo celda para "Saldo Anterior" que ocupa 3 columnas
                subText = HelperPdf.GeneraParrafo(
                               $"Saldo Anterior",
                               normalBold,
                               Element.ALIGN_LEFT,
                               3, 3,
                               true, // especificar color
                               BaseColor.Black // color negro
                           );
                PdfPCell celdaSaldoAnteriorTitulo = new PdfPCell(subText)
                {
                    Colspan = 3,
                    Border = Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(250, 248, 240), // Color claro para saldo anterior
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5
                };
                tablaSaldoAnterior.AddCell(celdaSaldoAnteriorTitulo);

                // Columna "Debe" vacía
                PdfPCell celdaDebeVacia = new PdfPCell(new Phrase("", normal))
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(250, 248, 240),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaSaldoAnterior.AddCell(celdaDebeVacia);

                // Columna "Haber" vacía
                PdfPCell celdaHaberVacia = new PdfPCell(new Phrase("", normal))
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(250, 248, 240),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaSaldoAnterior.AddCell(celdaHaberVacia);

                // Columna "Saldo" con el valor
                subText = HelperPdf.GeneraParrafo(
                              saldoInicial.ToString("N2"),
                              normalBold,
                              Element.ALIGN_RIGHT,
                              3, 3,
                              true, // especificar color
                              BaseColor.Black // color negro
                          );
                PdfPCell celdaSaldoAnteriorValor = new PdfPCell(subText)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(250, 248, 240),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaSaldoAnterior.AddCell(celdaSaldoAnteriorValor);

                // Agregar la tabla de saldo anterior al documento
                pdf.Add(tablaSaldoAnterior);

                // Procesar registros agrupados por fecha
                bool colorAlternado = true;

                foreach (var item in registros)
                {
                    // Si es una nueva fecha, insertamos encabezado y subtotales de la fecha anterior
                    if (fechaActual == null || item.Dia_fecha.Date != fechaActual.Value.Date)
                    {
                        // Si ya teníamos una fecha anterior, mostramos su subtotal
                        if (fechaActual != null)
                        {
                            // Crear una tabla para el subtotal del día
                            PdfPTable tablaSubtotal = new PdfPTable(6);
                            tablaSubtotal.SetWidths(anchos);
                            tablaSubtotal.WidthPercentage = 100;

                            // Texto subtotal
                            subText = HelperPdf.GeneraParrafo(
                                $"Subtotal del día {fechaActual.Value:dd/MM/yyyy}",
                                normalBold,
                                Element.ALIGN_LEFT,
                                3, 3,
                                true, // especificar color
                                BaseColor.Black // color negro
                            );

                            PdfPCell celdaSubtotalTexto = new PdfPCell(subText)
                            {
                                Colspan = 3,
                                Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                                BackgroundColor = new BaseColor(248, 246, 239), // Color claro para subtotales
                                HorizontalAlignment = Element.ALIGN_LEFT,
                                Padding = 5
                            };
                            tablaSubtotal.AddCell(celdaSubtotalTexto);

                            // Valor debe
                            subText = HelperPdf.GeneraParrafo(
                                subTotalDebe.ToString("N2"),
                                normalBold,
                                Element.ALIGN_RIGHT,
                                3, 3,
                                true, // especificar color
                                BaseColor.Black // color negro
                            );
                            PdfPCell celdaSubtotalDebe = new PdfPCell(subText)
                            {
                                Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                                BackgroundColor = new BaseColor(248, 246, 239),
                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                Padding = 5
                            };
                            tablaSubtotal.AddCell(celdaSubtotalDebe);

                            // Valor haber
                            subText = HelperPdf.GeneraParrafo(
                               subTotalHaber.ToString("N2"),
                               normalBold,
                               Element.ALIGN_RIGHT,
                               3, 3,
                               true, // especificar color
                               BaseColor.Black // color negro
                           );
                            PdfPCell celdaSubtotalHaber = new PdfPCell(subText)
                            {
                                Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                                BackgroundColor = new BaseColor(248, 246, 239),
                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                Padding = 5
                            };
                            tablaSubtotal.AddCell(celdaSubtotalHaber);

                            // Celda vacía para saldo                           
                            PdfPCell celdaSubtotalSaldo = new PdfPCell(new Phrase("", normal))
                            {
                                Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                                BackgroundColor = new BaseColor(248, 246, 239),
                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                Padding = 5
                            };
                            tablaSubtotal.AddCell(celdaSubtotalSaldo);

                            // Agregar tabla de subtotal al documento
                            pdf.Add(tablaSubtotal);
                        }

                        // Reiniciar variables para la nueva fecha
                        fechaActual = item.Dia_fecha;
                        subTotalDebe = 0;
                        subTotalHaber = 0;

                        // Crear tabla para el encabezado de grupo por fecha
                        PdfPTable tablaFecha = new PdfPTable(1);
                        tablaFecha.WidthPercentage = 100;

                        // Encabezado con la fecha
                        PdfPCell celdaFecha = new PdfPCell(new Phrase($"  {fechaActual.Value.ToString("dddd, dd 'de' MMMM 'de' yyyy")}", normalBold))
                        {
                            Border = Rectangle.NO_BORDER,
                            BackgroundColor = new BaseColor(250, 245, 230), // Color dorado claro para encabezado de fecha
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Padding = 5
                        };

                        // Agregar ícono de calendario
                        Chunk icono = new Chunk("\u25B6 ", normal); // Símbolo flecha derecha
                        icono.Font.Size = 8;
                        Paragraph paraFecha = new Paragraph();
                        paraFecha.Add(icono);
                        paraFecha.Add(new Chunk($"{fechaActual.Value.ToString("dddd, dd 'de' MMMM 'de' yyyy")}", normalBold));
                        celdaFecha = new PdfPCell(paraFecha)
                        {
                            Border = Rectangle.NO_BORDER,
                            BackgroundColor = new BaseColor(250, 245, 230), // Color dorado claro para encabezado de fecha
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Padding = 5
                        };

                        tablaFecha.AddCell(celdaFecha);

                        // Agregar tabla de fecha al documento
                        pdf.Add(tablaFecha);
                    }

                    // Actualizar subtotales
                    subTotalDebe += item.Dia_debe;
                    subTotalHaber += item.Dia_haber;
                    totalDebe += item.Dia_debe;
                    totalHaber += item.Dia_haber;

                    // Crear tabla para el registro
                    PdfPTable tablaRegistro = new PdfPTable(6);
                    tablaRegistro.SetWidths(anchos);
                    tablaRegistro.WidthPercentage = 100;

                    // Color alternado para las filas
                    BaseColor colorFondo = colorAlternado ?
                        new BaseColor(255, 255, 255) :
                        new BaseColor(252, 251, 247); // Color muy claro para filas alternadas

                    colorAlternado = !colorAlternado;

                    // Fecha
                    PdfPCell celdaFechaReg = new PdfPCell(new Phrase(item.Dia_fecha.ToString("dd/MM/yyyy"), normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaFechaReg);

                    // Nº Movimiento
                    PdfPCell celdaMovimiento = new PdfPCell(new Phrase(item.Dia_movi, normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaMovimiento);

                    // Concepto
                    PdfPCell celdaConcepto = new PdfPCell(new Phrase(item.Dia_desc_asiento, normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaConcepto);

                    // Debe
                    PdfPCell celdaDebe = new PdfPCell(new Phrase(item.Dia_debe > 0 ? item.Dia_debe.ToString("N2") : "0.00", normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaDebe);

                    // Haber
                    PdfPCell celdaHaber = new PdfPCell(new Phrase(item.Dia_haber > 0 ? item.Dia_haber.ToString("N2") : "0.00", normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaHaber);

                    // Saldo
                    PdfPCell celdaSaldo = new PdfPCell(new Phrase(item.Dia_saldo.ToString("N2"), normal))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LightGray,
                        BackgroundColor = colorFondo,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaRegistro.AddCell(celdaSaldo);

                    // Agregar tabla de registro al documento
                    pdf.Add(tablaRegistro);
                }

                // Mostrar el subtotal del último día
                if (fechaActual != null)
                {
                    // Crear una tabla para el subtotal del día
                    PdfPTable tablaSubtotal = new PdfPTable(6);
                    tablaSubtotal.SetWidths(anchos);
                    tablaSubtotal.WidthPercentage = 100;

                    // Texto subtotal
                    subText = HelperPdf.GeneraParrafo(
                                $"Subtotal del día {fechaActual.Value:dd/MM/yyyy}",
                                normalBold,
                                Element.ALIGN_LEFT,
                                3, 3,
                                true, // especificar color
                                BaseColor.Black // color negro
                            );
                    PdfPCell celdaSubtotalTexto = new PdfPCell(subText)
                    {
                        Colspan = 3,
                        Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                        BackgroundColor = new BaseColor(248, 246, 239), // Color claro para subtotales
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 5
                    };
                    tablaSubtotal.AddCell(celdaSubtotalTexto);

                    // Valor debe
                    subText = HelperPdf.GeneraParrafo(
                               subTotalDebe.ToString("N2"),
                               normalBold,
                               Element.ALIGN_RIGHT,
                               3, 3,
                               true, // especificar color
                               BaseColor.Black // color negro
                           );
                    PdfPCell celdaSubtotalDebe = new PdfPCell(subText)
                    {
                        Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                        BackgroundColor = new BaseColor(248, 246, 239),
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaSubtotal.AddCell(celdaSubtotalDebe);

                    // Valor haber
                    subText = HelperPdf.GeneraParrafo(
                              subTotalHaber.ToString("N2"),
                              normalBold,
                              Element.ALIGN_RIGHT,
                              3, 3,
                              true, // especificar color
                              BaseColor.Black // color negro
                          );
                    PdfPCell celdaSubtotalHaber = new PdfPCell(subText)
                    {
                        Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                        BackgroundColor = new BaseColor(248, 246, 239),
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaSubtotal.AddCell(celdaSubtotalHaber);

                    // Celda vacía para saldo
                    PdfPCell celdaSubtotalSaldo = new PdfPCell(new Phrase("", normal))
                    {
                        Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                        BackgroundColor = new BaseColor(248, 246, 239),
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    tablaSubtotal.AddCell(celdaSubtotalSaldo);

                    // Agregar tabla de subtotal al documento
                    pdf.Add(tablaSubtotal);
                }

                // Crear tabla para el total general
                PdfPTable tablaTotal = new PdfPTable(6);
                tablaTotal.SetWidths(anchos);
                tablaTotal.WidthPercentage = 100;

                // Texto total
                subText = HelperPdf.GeneraParrafo(
                               "Total General",
                                normalBold,
                                Element.ALIGN_LEFT,
                                3, 3,
                                true, // especificar color
                                BaseColor.Black // color negro
                            );
                PdfPCell celdaTotalTexto = new PdfPCell(subText)
                {
                    Colspan = 3,
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(240, 228, 195), // Color más oscuro para total
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5
                };
                tablaTotal.AddCell(celdaTotalTexto);

                // Valor total debe
                subText = HelperPdf.GeneraParrafo(
                             totalDebe.ToString("N2"),
                             normalBold,
                             Element.ALIGN_RIGHT,
                             3, 3,
                             true, // especificar color
                             BaseColor.Black // color negro
                         );
                PdfPCell celdaTotalDebe = new PdfPCell(subText)
                {
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(240, 228, 195),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaTotal.AddCell(celdaTotalDebe);

                // Valor total haber
                subText = HelperPdf.GeneraParrafo(
                            totalHaber.ToString("N2"),
                            normalBold,
                            Element.ALIGN_RIGHT,
                            3, 3,
                            true, // especificar color
                            BaseColor.Black // color negro
                        );
                PdfPCell celdaTotalHaber = new PdfPCell(subText)
                {
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(240, 228, 195),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaTotal.AddCell(celdaTotalHaber);

                // Saldo final

                decimal saldoFinal = registros.Last().Dia_saldo;
                subText = HelperPdf.GeneraParrafo(
                            saldoFinal.ToString("N2"),
                            normalBold,
                            Element.ALIGN_RIGHT,
                            3, 3,
                            true, // especificar color
                            BaseColor.Black // color negro
                        );
                PdfPCell celdaSaldoFinal = new PdfPCell(subText)
                {
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                    BackgroundColor = new BaseColor(240, 228, 195),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                };
                tablaTotal.AddCell(celdaSaldoFinal);

                // Agregar tabla de total al documento
                pdf.Add(tablaTotal);

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

                throw new NegocioException($"Se produjo un error al intentar generar la Impresión del Libro Mayor para la cuenta {ctaId}. Para mayores datos ver el log.");
            }

        }


        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<LMayorRegListaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Fecha = x.Dia_fecha.ToShortDateString(),
                Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                Concepto = x.Dia_desc,
                Debe = x.Dia_debe,
                Haber = x.Dia_haber,
                Saldo = x.Dia_saldo
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<LMayorRegListaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Fecha = x.Dia_fecha.ToShortDateString(),
                Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                Concepto = x.Dia_desc,
                Debe = x.Dia_debe,
                Haber = x.Dia_haber,
                Saldo = x.Dia_saldo
            }).ToList();
            #endregion
            return GeneraFileXLS(regs, _titulos, _campos);
        }



        private List<LMayorRegListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
        {

            var eje_nro = solicitud.Parametros.GetValueOrDefault("eje_nro", "");
            var ccb_id = solicitud.Parametros.GetValueOrDefault("ccb_id", "");
            var ccb_desc = solicitud.Parametros.GetValueOrDefault("ccb_desc", "");
            var incluirTemporales = solicitud.Parametros.GetValueOrDefault("incluirTemporales", "").ToBoolean();
            var rango = solicitud.Parametros.GetValueOrDefault("rango", "").ToBoolean();
            var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
            var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();



            id = ccb_id;
            return _lMayorSv.ObtenerLibroMayor(eje_nro.ToInt(),
                ccb_id,
                rango, desde, hasta, incluirTemporales, 99999, 1, "");
        }
    }
}
