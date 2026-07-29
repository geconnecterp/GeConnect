using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
    internal static class FacturaBase
    {
        private const float FooterTopY = 165f;
        private const float FooterBottomY = 52f;
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

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
            Font fuenteChica)
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
            DibujarPieFiscalComun(writer, pdf, encabezado, paginaActual, totalPaginas, fuenteChica);
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
            Font fuenteChica)
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

            DibujarPieFiscalComun(writer, pdf, encabezado, paginaActual, totalPaginas, fuenteChica);
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

        private static void DibujarPieFiscalComun(PdfWriter writer, Document pdf, FeResDto encabezado, int paginaActual, int totalPaginas, Font fuenteChica)
        {
            PdfPTable tabla = new PdfPTable(3);
            tabla.TotalWidth = pdf.PageSize.Width - pdf.LeftMargin - pdf.RightMargin;
            tabla.SetWidths(new float[] { 30f, 35f, 35f });

            AgregarCeldaSinBorde(tabla, "AFIP", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tabla, $"Pág. {paginaActual} de {totalPaginas}", fuenteChica, Element.ALIGN_CENTER);

            PdfPTable cae = new PdfPTable(1);
            cae.WidthPercentage = 100;
            AgregarCeldaSinBorde(cae, $"CAE N°: {encabezado.cm_cae}", fuenteChica, Element.ALIGN_RIGHT);
            var caeVto = encabezado.cm_cae_vto.HasValue ? encabezado.cm_cae_vto.Value.ToString("dd/MM/yyyy") : string.Empty;
            AgregarCeldaSinBorde(cae, $"Fecha de Vto. de CAE: {caeVto}", fuenteChica, Element.ALIGN_RIGHT);
            PdfPCell celdaCae = new PdfPCell(cae);
            celdaCae.Border = Rectangle.NO_BORDER;
            tabla.AddCell(celdaCae);

            tabla.WriteSelectedRows(0, -1, pdf.LeftMargin, FooterBottomY - 5f, writer.DirectContent);
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