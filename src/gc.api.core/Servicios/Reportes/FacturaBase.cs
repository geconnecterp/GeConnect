using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace gc.api.core.Servicios.Reportes
{
    internal static class FacturaBase
    {
        private const float FooterTopY = 165f;
        private const float FooterBottomY = 86f;
        private const string ArcaQrBaseUrl = "https://www.arca.gob.ar/fe/qr/?p=";
        private const long CaeEjemploDesarrollo = 70417054367476;
        private static readonly DateTime CaeVtoEjemploDesarrollo = new(2020, 10, 23);
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
        private static readonly JsonSerializerOptions ArcaQrJsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        internal static Font CrearFuente(int size, bool bold = false)
        {
            return HelperPdf.DefineFontWithStyle("Arial", size, bold ? Font.BOLD : Font.NORMAL, 0, 0, 0);
        }

        internal static string ObtenerLeyendaImpresion(ReporteSolicitudDto solicitud)
        {
            if (solicitud.Parametros.TryGetValue("leyenda_impresion", out var leyenda) ||
                solicitud.Parametros.TryGetValue("leyenda", out leyenda) ||
                solicitud.Parametros.TryGetValue("tipo_impresion", out leyenda))
            {
                leyenda = (leyenda ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(leyenda))
                {
                    return leyenda.ToUpperInvariant();
                }
            }

            return "ORIGINAL";
        }

        internal static string FormatearImporte(decimal importe)
        {
            return importe.ToString("N2", Cultura);
        }

        internal static string ObtenerTituloComprobante(FeResDto encabezado)
        {
            var descripcion = (encabezado.tco_desc ?? string.Empty).Trim();

            if (descripcion.Contains("Factura", StringComparison.OrdinalIgnoreCase))
            {
                return "TICKET FACTURA";
            }

            return string.IsNullOrWhiteSpace(descripcion) ? "COMPROBANTE" : descripcion.ToUpperInvariant();
        }

        internal static string ObtenerPuntoVenta(FeResDto encabezado)
        {
            var compte = (encabezado.cm_compte ?? string.Empty).Trim();
            var partes = compte.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length > 0 ? partes[0] : (encabezado.caja_id ?? string.Empty).Trim();
        }

        internal static string ObtenerNumeroComprobante(FeResDto encabezado)
        {
            var compte = (encabezado.cm_compte ?? string.Empty).Trim();
            var partes = compte.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length > 1 ? partes[1] : compte;
        }

        internal static string ObtenerDomicilioEmpresa(FeResDto encabezado)
        {
            var domicilio = (encabezado.emp_domicilio ?? string.Empty).Trim();
            var administracion = (encabezado.adm_direccion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(domicilio) || domicilio == "...")
            {
                return administracion;
            }

            if (string.IsNullOrWhiteSpace(administracion))
            {
                return domicilio;
            }

            if (domicilio.Contains(administracion, StringComparison.OrdinalIgnoreCase))
            {
                return domicilio;
            }

            return $"{domicilio} - {administracion}";
        }

        internal static int CalcularTotalPaginas(int totalFilas, int filasPaginaComun, int filasUltimaPagina)
        {
            if (totalFilas <= filasUltimaPagina)
            {
                return 1;
            }

            var filasPrevias = totalFilas - filasUltimaPagina;
            return (int)Math.Ceiling(filasPrevias / (decimal)filasPaginaComun) + 1;
        }

        internal static int ObtenerCantidadFilasPagina(int paginaActual, int totalPaginas, int filasPaginaComun, int filasUltimaPagina)
        {
            return paginaActual == totalPaginas ? filasUltimaPagina : filasPaginaComun;
        }

        internal static void GenerarLeyendaComprobante(Document pdf, string leyendaImpresion)
        {
            PdfPTable tabla = new PdfPTable(1);
            tabla.WidthPercentage = 100;

            PdfPCell celda = new PdfPCell(new Phrase(leyendaImpresion, CrearFuente(11, true)));
            celda.Border = Rectangle.BOX;
            celda.HorizontalAlignment = Element.ALIGN_CENTER;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.Padding = 4f;
            tabla.AddCell(celda);

            pdf.Add(tabla);
        }

        internal static void GenerarCabeceraFactura(
            Document pdf,
            FeResDto encabezado,
            Image logo,
            Font fuenteTitulo,
            Font fuenteSubtitulo,
            Font fuenteNormal,
            Font fuenteChica,
            string leyendaImpresion)
        {
            GenerarLeyendaComprobante(pdf, leyendaImpresion);

            PdfPTable tablaCabecera = new PdfPTable(3);
            tablaCabecera.WidthPercentage = 100;
            tablaCabecera.SetWidths(new float[] { 44f, 10f, 46f });

            PdfPTable tablaEmpresa = new PdfPTable(1);
            tablaEmpresa.WidthPercentage = 100;

            PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
            celdaLogo.Border = Rectangle.NO_BORDER;
            celdaLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaLogo.FixedHeight = 30f;
            tablaEmpresa.AddCell(celdaLogo);

            AgregarCeldaSinBorde(tablaEmpresa, $"Razón Social: {encabezado.emp_razon_social}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaEmpresa, $"Domicilio Comercial: {ObtenerDomicilioEmpresa(encabezado)}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaEmpresa, $"Condición frente al IVA: {encabezado.afip_desc_emp}", fuenteChica, Element.ALIGN_LEFT);

            PdfPCell celdaEmpresa = new PdfPCell(tablaEmpresa);
            celdaEmpresa.Border = Rectangle.BOX;
            celdaEmpresa.Padding = 3f;
            celdaEmpresa.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaEmpresa);

            PdfPTable tablaLetra = new PdfPTable(1);
            tablaLetra.WidthPercentage = 100;

            PdfPCell celdaLetra = new PdfPCell(new Phrase(encabezado.tco_letra ?? string.Empty, fuenteTitulo));
            celdaLetra.Border = Rectangle.NO_BORDER;
            celdaLetra.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLetra.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaLetra.PaddingTop = 8f;
            tablaLetra.AddCell(celdaLetra);

            PdfPCell celdaTipo = new PdfPCell(new Phrase($"COD. {encabezado.tco_id}", fuenteChica));
            celdaTipo.Border = Rectangle.NO_BORDER;
            celdaTipo.HorizontalAlignment = Element.ALIGN_CENTER;
            tablaLetra.AddCell(celdaTipo);

            PdfPCell celdaContenedoraLetra = new PdfPCell(tablaLetra);
            celdaContenedoraLetra.Border = Rectangle.BOX;
            celdaContenedoraLetra.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaContenedoraLetra);

            PdfPTable tablaDatos = new PdfPTable(1);
            tablaDatos.WidthPercentage = 100;
            AgregarCeldaSinBorde(tablaDatos, ObtenerTituloComprobante(encabezado), fuenteSubtitulo, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Punto de Venta: {ObtenerPuntoVenta(encabezado)}          Comp. Nro: {ObtenerNumeroComprobante(encabezado)}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Emisión: {encabezado.cm_fecha:dd/MM/yyyy}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"CUIT: {encabezado.emp_cuit}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Ingresos Brutos: {encabezado.emp_ib_nro}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Inicio de Actividades: {encabezado.emp_inicio_act:dd/MM/yyyy}", fuenteChica, Element.ALIGN_LEFT);

            PdfPCell celdaContenedoraDatos = new PdfPCell(tablaDatos);
            celdaContenedoraDatos.Border = Rectangle.BOX;
            celdaContenedoraDatos.Padding = 3f;
            celdaContenedoraDatos.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaContenedoraDatos);

            pdf.Add(tablaCabecera);
        }

        internal static void GenerarDatosCliente(Document pdf, FeResDto encabezado, Font fuenteNormal, Font fuenteNormalBold)
        {
            PdfPTable tablaCliente = new PdfPTable(2);
            tablaCliente.WidthPercentage = 100;
            tablaCliente.SetWidths(new float[] { 24f, 76f });

            AgregarFilaDatos(tablaCliente, "CUIT:", encabezado.cm_cuit ?? string.Empty, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Apellido y Nombre / Razón Social:", encabezado.cm_nombre ?? string.Empty, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición frente al IVA:", encabezado.afip_desc_cli ?? string.Empty, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Domicilio Comercial:", encabezado.cm_domicilio ?? string.Empty, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición de Venta:", string.Empty, fuenteNormalBold, fuenteNormal);

            PdfPCell celdaCliente = new PdfPCell(tablaCliente);
            celdaCliente.Border = Rectangle.BOX;
            celdaCliente.Padding = 3f;

            PdfPTable wrapper = new PdfPTable(1);
            wrapper.WidthPercentage = 100;
            wrapper.AddCell(celdaCliente);

            pdf.Add(wrapper);
        }

        internal static PdfPTable CrearTablaProductos(float[] anchos, string[] encabezados, Font fuenteNormalBold)
        {
            PdfPTable tabla = new PdfPTable(anchos.Length);
            tabla.WidthPercentage = 100;
            tabla.SetWidths(anchos);

            foreach (var encabezado in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(encabezado, fuenteNormalBold));
                celda.BackgroundColor = new BaseColor(235, 235, 235);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                celda.Padding = 2f;
                tabla.AddCell(celda);
            }

            return tabla;
        }

        internal static void AgregarCeldaProducto(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.BOTTOM_BORDER;
            celda.BorderColor = BaseColor.LightGray;
            celda.HorizontalAlignment = alineacion;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.Padding = 1.5f;
            tabla.AddCell(celda);
        }

        internal static void DibujarPieFacturaB(
            PdfWriter writer,
            Document pdf,
            FeResDto encabezado,
            List<FePerResDto> datosPercepciones,
            int paginaActual,
            int totalPaginas,
            Font fuenteNormal,
            Font fuenteNormalBold,
            Font fuenteChica,
            ILogger? logger = null)
        {
            var canvas = writer.DirectContent;
            DibujarLineasPie(canvas, pdf);

            PdfPTable tablaTotales = new PdfPTable(2);
            tablaTotales.TotalWidth = 230f;
            tablaTotales.SetWidths(new float[] { 68f, 32f });

            var subtotal = encabezado.cm_gravado + encabezado.cm_no_gravado + encabezado.cm_exento + encabezado.cm_iva;
            var totalPercepciones = datosPercepciones?.Sum(p => p.percepcion ?? 0) ?? 0;

            AgregarLineaTotal(tablaTotales, "Sub Total:", subtotal, fuenteNormal, false);
            AgregarLineaTotal(tablaTotales, "Importe Otros Tributos:", totalPercepciones, fuenteNormal, false);
            AgregarLineaTotal(tablaTotales, "Rec/Dto:", encabezado.cm_dto, fuenteNormal, false);
            AgregarLineaTotal(tablaTotales, "Importe Total:", encabezado.cm_total, fuenteNormalBold, true);

            tablaTotales.WriteSelectedRows(0, -1, pdf.PageSize.Width - pdf.RightMargin - 230f, FooterTopY - 10f, canvas);
            DibujarPieFiscalComun(writer, pdf, encabezado, paginaActual, totalPaginas, fuenteChica, logger);
        }

        internal static void DibujarPieFacturaA(
            PdfWriter writer,
            Document pdf,
            FeResDto encabezado,
            List<FeIvaResDto> datosIva,
            List<FePerResDto> datosPercepciones,
            int paginaActual,
            int totalPaginas,
            Font fuenteNormal,
            Font fuenteNormalBold,
            Font fuenteChica,
            ILogger? logger = null)
        {
            var canvas = writer.DirectContent;
            DibujarLineasPie(canvas, pdf);

            PdfPTable resumen = new PdfPTable(6);
            resumen.TotalWidth = pdf.PageSize.Width - pdf.LeftMargin - pdf.RightMargin;
            resumen.SetWidths(new float[] { 16f, 12f, 16f, 12f, 16f, 12f });
            AgregarCeldaResumen(resumen, "Subtotal:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumen(resumen, FormatearImporte(encabezado.cm_gravado), fuenteNormalBold, Element.ALIGN_RIGHT);
            AgregarCeldaResumen(resumen, "Percep.:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumen(resumen, FormatearImporte(encabezado.cm_percepciones), fuenteNormalBold, Element.ALIGN_RIGHT);
            AgregarCeldaResumen(resumen, "Rec/Dto.:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumen(resumen, FormatearImporte(encabezado.cm_dto), fuenteNormalBold, Element.ALIGN_RIGHT);
            resumen.WriteSelectedRows(0, -1, pdf.LeftMargin, FooterTopY + 10f, canvas);

            PdfPTable tributos = CrearTablaTributos(datosPercepciones, fuenteNormalBold, fuenteChica);
            tributos.TotalWidth = 270f;
            tributos.WriteSelectedRows(0, -1, pdf.LeftMargin, FooterTopY - 12f, canvas);

            PdfPTable totales = new PdfPTable(2);
            totales.TotalWidth = 230f;
            totales.SetWidths(new float[] { 68f, 32f });
            foreach (var lineaIva in (datosIva ?? new List<FeIvaResDto>()).OrderBy(i => i.orden))
            {
                AgregarLineaTotal(totales, lineaIva.concepto ?? string.Empty, lineaIva.importe ?? 0, fuenteNormal, false);
            }
            AgregarLineaTotal(totales, "Importe Total:", encabezado.cm_total, fuenteNormalBold, true);
            totales.WriteSelectedRows(0, -1, pdf.PageSize.Width - pdf.RightMargin - 230f, FooterTopY - 12f, canvas);

            DibujarPieFiscalComun(writer, pdf, encabezado, paginaActual, totalPaginas, fuenteChica, logger);
        }

        private static void DibujarLineasPie(PdfContentByte canvas, Document pdf)
        {
            canvas.SetLineWidth(0.5f);
            canvas.MoveTo(pdf.LeftMargin, FooterTopY);
            canvas.LineTo(pdf.PageSize.Width - pdf.RightMargin, FooterTopY);
            canvas.Stroke();
            canvas.MoveTo(pdf.LeftMargin, FooterBottomY);
            canvas.LineTo(pdf.PageSize.Width - pdf.RightMargin, FooterBottomY);
            canvas.Stroke();
        }

        private static void DibujarPieFiscalComun(PdfWriter writer, Document pdf, FeResDto encabezado, int paginaActual, int totalPaginas, Font fuenteChica, ILogger? logger = null)
        {
            PdfPTable tabla = new PdfPTable(3);
            tabla.TotalWidth = pdf.PageSize.Width - pdf.LeftMargin - pdf.RightMargin;
            tabla.SetWidths(new float[] { 30f, 35f, 35f });

            PdfPCell celdaQr = new PdfPCell();
            celdaQr.Border = Rectangle.NO_BORDER;
            celdaQr.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaQr.VerticalAlignment = Element.ALIGN_TOP;
            celdaQr.Padding = 0f;

            var qr = GenerarQrArca(encabezado, logger);
            if (qr != null)
            {
                qr.ScaleAbsolute(58f, 58f);
                celdaQr.AddElement(qr);
                logger?.LogInformation(
                    "FacturaBase QR ARCA agregado al pie. tco_id={TcoId}; cm_compte={CmCompte}; cm_repetido={CmRepetido}; pagina={Pagina}/{TotalPaginas}",
                    encabezado.tco_id,
                    encabezado.cm_compte,
                    encabezado.cm_repetido,
                    paginaActual,
                    totalPaginas);
            }
            else
            {
                celdaQr.AddElement(new Phrase("ARCA", fuenteChica));
            }
            tabla.AddCell(celdaQr);

            PdfPTable centro = new PdfPTable(1);
            centro.WidthPercentage = 100;
            AgregarCeldaSinBorde(centro, "ARCA", CrearFuente(15, true), Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(centro, "Comprobante Autorizado", CrearFuente(6, true), Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(centro, "Esta Administracion Federal no se responsabiliza por los datos ingresados en el detalle de la operacion", CrearFuente(4, true), Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(centro, $"Pág. {paginaActual}/{totalPaginas}", fuenteChica, Element.ALIGN_CENTER);
            PdfPCell celdaCentro = new PdfPCell(centro);
            celdaCentro.Border = Rectangle.NO_BORDER;
            celdaCentro.Padding = 0f;
            tabla.AddCell(celdaCentro);

            PdfPTable cae = new PdfPTable(1);
            cae.WidthPercentage = 100;
            var caeTexto = ObtenerCaeParaImpresion(encabezado, logger);
            AgregarCeldaSinBorde(cae, $"CAE N°: {caeTexto}", fuenteChica, Element.ALIGN_RIGHT);
            var caeVto = ObtenerCaeVtoParaImpresion(encabezado, logger);
            AgregarCeldaSinBorde(cae, $"Fecha de Vto. de CAE: {caeVto}", fuenteChica, Element.ALIGN_RIGHT);
            PdfPCell celdaCae = new PdfPCell(cae);
            celdaCae.Border = Rectangle.NO_BORDER;
            tabla.AddCell(celdaCae);

            tabla.WriteSelectedRows(0, -1, pdf.LeftMargin, FooterBottomY - 5f, writer.DirectContent);
        }

        private static Image? GenerarQrArca(FeResDto encabezado, ILogger? logger = null)
        {
            var url = GenerarUrlQrArca(encabezado, logger);
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                return Image.GetInstance(ArcaQrHelper.GenerarPng(url));
            }
            catch (Exception ex)
            {
                logger?.LogError(
                    ex,
                    "FacturaBase QR ARCA no se pudo renderizar. tco_id={TcoId}; cm_compte={CmCompte}; cm_repetido={CmRepetido}; url_length={UrlLength}",
                    encabezado.tco_id,
                    encabezado.cm_compte,
                    encabezado.cm_repetido,
                    url.Length);
                return null;
            }
        }

        private static string GenerarUrlQrArca(FeResDto encabezado, ILogger? logger = null)
        {
            var puntoVentaTexto = ObtenerPuntoVenta(encabezado);
            var numeroComprobanteTexto = ObtenerNumeroComprobante(encabezado);
            var cuitEmisorTexto = ObtenerSoloDigitos(encabezado.emp_cuit);
            var tipoComprobanteTexto = ObtenerSoloDigitos(encabezado.tco_id);
            var numeroComprobanteSoloDigitos = ObtenerSoloDigitos(numeroComprobanteTexto);
            var codigoAutorizacionTexto = ObtenerSoloDigitos(ObtenerCaeParaImpresion(encabezado, logger));

            logger?.LogInformation(
                "FacturaBase QR ARCA datos de entrada. tco_id={TcoId}; tco_desc={TcoDesc}; cm_compte={CmCompte}; cm_repetido={CmRepetido}; emp_cuit={EmpCuit}; puntoVenta={PuntoVenta}; numero={Numero}; cm_total={Total}; mon_codigo={Moneda}; cm_cae={Cae}; cm_cae_vto={CaeVto}; cm_cuit={DocReceptor}",
                encabezado.tco_id,
                encabezado.tco_desc,
                encabezado.cm_compte,
                encabezado.cm_repetido,
                encabezado.emp_cuit,
                puntoVentaTexto,
                numeroComprobanteTexto,
                encabezado.cm_total,
                encabezado.mon_codigo,
                encabezado.cm_cae,
                encabezado.cm_cae_vto,
                encabezado.cm_cuit);

            if (!long.TryParse(cuitEmisorTexto, out var cuitEmisor) || cuitEmisor == 0)
            {
                logger?.LogWarning("FacturaBase QR ARCA omitido: CUIT emisor invalido. emp_cuit={EmpCuit}; normalizado={Normalizado}", encabezado.emp_cuit, cuitEmisorTexto);
                return string.Empty;
            }

            if (!int.TryParse(puntoVentaTexto, out var puntoVenta) || puntoVenta == 0)
            {
                logger?.LogWarning("FacturaBase QR ARCA omitido: punto de venta invalido. cm_compte={CmCompte}; caja_id={CajaId}; puntoVenta={PuntoVenta}", encabezado.cm_compte, encabezado.caja_id, puntoVentaTexto);
                return string.Empty;
            }

            if (!int.TryParse(tipoComprobanteTexto, out var tipoComprobante) || tipoComprobante == 0)
            {
                logger?.LogWarning("FacturaBase QR ARCA omitido: tipo de comprobante invalido. tco_id={TcoId}; normalizado={Normalizado}", encabezado.tco_id, tipoComprobanteTexto);
                return string.Empty;
            }

            if (!long.TryParse(numeroComprobanteSoloDigitos, out var numeroComprobante) || numeroComprobante == 0)
            {
                logger?.LogWarning("FacturaBase QR ARCA omitido: numero de comprobante invalido. cm_compte={CmCompte}; numero={Numero}; normalizado={Normalizado}", encabezado.cm_compte, numeroComprobanteTexto, numeroComprobanteSoloDigitos);
                return string.Empty;
            }

            if (!long.TryParse(codigoAutorizacionTexto, out var codigoAutorizacion) || codigoAutorizacion == 0)
            {
                logger?.LogWarning("FacturaBase QR ARCA omitido: codigo de autorizacion CAE/CAEA invalido. cm_cae={Cae}; normalizado={Normalizado}", encabezado.cm_cae, codigoAutorizacionTexto);
                return string.Empty;
            }

            var dto = new ArcaQrComprobanteDto
            {
                Ver = 1,
                Fecha = encabezado.cm_fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Cuit = cuitEmisor,
                PtoVta = puntoVenta,
                TipoCmp = tipoComprobante,
                NroCmp = numeroComprobante,
                Importe = encabezado.cm_total,
                Moneda = string.IsNullOrWhiteSpace(encabezado.mon_codigo) ? "PES" : encabezado.mon_codigo.Trim(),
                Ctz = 1m,
                TipoCodAut = "E",
                CodAut = codigoAutorizacion
            };

            var documentoReceptor = ObtenerSoloDigitos(encabezado.cm_cuit);
            if (long.TryParse(documentoReceptor, out var nroDocReceptor) && nroDocReceptor > 0)
            {
                dto.NroDocRec = nroDocReceptor;
                dto.TipoDocRec = documentoReceptor.Length == 11 ? 80 : documentoReceptor.Length == 8 ? 96 : null;
            }

            var json = JsonSerializer.Serialize(dto, ArcaQrJsonOptions);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            logger?.LogInformation("FacturaBase QR ARCA generado. json={Json}; base64={Base64}; url={Url}", json, base64, ArcaQrBaseUrl + base64);
            return ArcaQrBaseUrl + base64;
        }

        private static string ObtenerSoloDigitos(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return new string(valor.Where(char.IsDigit).ToArray());
        }

        private static string ObtenerCaeParaImpresion(FeResDto encabezado, ILogger? logger = null)
        {
            var caeNormalizado = ObtenerSoloDigitos(encabezado.cm_cae);
            if (long.TryParse(caeNormalizado, out var cae) && cae > 0)
            {
                return caeNormalizado;
            }

            logger?.LogWarning(
                "FacturaBase usando CAE de ejemplo para desarrollo. tco_id={TcoId}; cm_compte={CmCompte}; cm_repetido={CmRepetido}; cm_cae_original={CaeOriginal}; cae_ejemplo={CaeEjemplo}",
                encabezado.tco_id,
                encabezado.cm_compte,
                encabezado.cm_repetido,
                encabezado.cm_cae,
                CaeEjemploDesarrollo);

            return CaeEjemploDesarrollo.ToString(CultureInfo.InvariantCulture);
        }

        private static string ObtenerCaeVtoParaImpresion(FeResDto encabezado, ILogger? logger = null)
        {
            if (encabezado.cm_cae_vto.HasValue && encabezado.cm_cae_vto.Value.Date > new DateTime(1900, 1, 1))
            {
                return encabezado.cm_cae_vto.Value.ToString("dd/MM/yyyy");
            }

            logger?.LogWarning(
                "FacturaBase usando vencimiento de CAE de ejemplo para desarrollo. tco_id={TcoId}; cm_compte={CmCompte}; cm_repetido={CmRepetido}; cm_cae_vto_original={CaeVtoOriginal}; cae_vto_ejemplo={CaeVtoEjemplo}",
                encabezado.tco_id,
                encabezado.cm_compte,
                encabezado.cm_repetido,
                encabezado.cm_cae_vto,
                CaeVtoEjemploDesarrollo);

            return CaeVtoEjemploDesarrollo.ToString("dd/MM/yyyy");
        }
        private static PdfPTable CrearTablaTributos(List<FePerResDto> datosPercepciones, Font fuenteNormalBold, Font fuenteChica)
        {
            PdfPTable tabla = new PdfPTable(4);
            tabla.SetWidths(new float[] { 50f, 17f, 17f, 16f });

            PdfPCell titulo = new PdfPCell(new Phrase("Otros Tributos", fuenteNormalBold));
            titulo.Colspan = 4;
            titulo.BackgroundColor = new BaseColor(235, 235, 235);
            titulo.HorizontalAlignment = Element.ALIGN_CENTER;
            titulo.Padding = 2f;
            tabla.AddCell(titulo);

            foreach (var encabezado in new[] { "Descripción", "Base", "Ali", "Importe" })
            {
                PdfPCell celda = new PdfPCell(new Phrase(encabezado, fuenteNormalBold));
                celda.BackgroundColor = new BaseColor(245, 245, 245);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 1.5f;
                tabla.AddCell(celda);
            }

            if (datosPercepciones != null && datosPercepciones.Any())
            {
                foreach (var percepcion in datosPercepciones.OrderBy(p => p.orden))
                {
                    AgregarCeldaProducto(tabla, percepcion.imp_des ?? string.Empty, fuenteChica, Element.ALIGN_LEFT);
                    AgregarCeldaProducto(tabla, FormatearImporte(percepcion.Base ?? 0), fuenteChica, Element.ALIGN_RIGHT);
                    AgregarCeldaProducto(tabla, FormatearImporte(percepcion.ali ?? 0), fuenteChica, Element.ALIGN_RIGHT);
                    AgregarCeldaProducto(tabla, FormatearImporte(percepcion.percepcion ?? 0), fuenteChica, Element.ALIGN_RIGHT);
                }
            }
            else
            {
                AgregarCeldaProducto(tabla, string.Empty, fuenteChica, Element.ALIGN_LEFT);
                AgregarCeldaProducto(tabla, FormatearImporte(0), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tabla, FormatearImporte(0), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tabla, FormatearImporte(0), fuenteChica, Element.ALIGN_RIGHT);
            }

            PdfPCell totalEtiqueta = new PdfPCell(new Phrase("Importe Otros Tributos:", fuenteNormalBold));
            totalEtiqueta.Colspan = 3;
            totalEtiqueta.Border = Rectangle.NO_BORDER;
            totalEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalEtiqueta.Padding = 1.5f;
            tabla.AddCell(totalEtiqueta);

            PdfPCell totalValor = new PdfPCell(new Phrase(FormatearImporte(datosPercepciones?.Sum(p => p.percepcion ?? 0) ?? 0), fuenteNormalBold));
            totalValor.Border = Rectangle.NO_BORDER;
            totalValor.HorizontalAlignment = Element.ALIGN_RIGHT;
            totalValor.Padding = 1.5f;
            tabla.AddCell(totalValor);

            return tabla;
        }

        internal static void AgregarCeldaSinBorde(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 1.2f;
            tabla.AddCell(celda);
        }

        private static void AgregarFilaDatos(PdfPTable tabla, string etiqueta, string valor, Font fuenteEtiqueta, Font fuenteValor)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuenteEtiqueta));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaEtiqueta.Padding = 1.2f;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(valor, fuenteValor));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaValor.Padding = 1.2f;
            tabla.AddCell(celdaValor);
        }

        private static void AgregarLineaTotal(PdfPTable tabla, string etiqueta, decimal valor, Font fuente, bool resaltar)
        {
            BaseColor? fondo = resaltar ? new BaseColor(235, 235, 235) : null;

            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuente));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaEtiqueta.Padding = 1.8f;
            if (fondo != null) celdaEtiqueta.BackgroundColor = fondo;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(FormatearImporte(valor), fuente));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaValor.Padding = 1.8f;
            if (fondo != null) celdaValor.BackgroundColor = fondo;
            tabla.AddCell(celdaValor);
        }

        private static void AgregarCeldaResumen(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.TOP_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 1f;
            tabla.AddCell(celda);
        }
    }
}
