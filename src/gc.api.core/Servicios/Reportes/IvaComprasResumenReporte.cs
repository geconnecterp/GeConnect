using ClosedXML.Excel;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes;

public static class IvaComprasResumenReporte
{
    public const int Id = (int)gc.infraestructura.Enumeraciones.InfoReporte.R105_LibroIvaComprasResumen;
    public const string MimeExcel = IvaVentasResumenReporte.MimeExcel;

    public static byte[] Excel(IvaComprasResumenLibro libro, CancellationToken ct = default)
    {
        libro.Validar();
        if (libro.Cantidad == 0) throw new ArgumentException("No hay filas para exportar.");
        using var wb = new XLWorkbook();
        foreach (var bloque in libro.Bloques)
        {
            var sufijo = bloque.Codigo == "BI" ? "BI" : "Otros";
            var control = wb.AddWorksheet($"Control {sufijo}");
            Encabezado(control, libro, bloque.Nombre, 4);
            string[] titulos = ["Cuenta", "Concepto", "Contabilidad", "Libro IVA"];
            for (var c = 0; c < titulos.Length; c++) control.Cell(4, c + 1).Value = titulos[c];
            var r = 5;
            if (bloque.Control.Count > 1_048_560 || bloque.Resumen.Count > 1_048_560)
                throw new InvalidOperationException("El conjunto excede la capacidad de Excel. No se genera un archivo parcial.");
            foreach (var fila in bloque.Control)
            {
                ct.ThrowIfCancellationRequested();
                control.Cell(r, 1).Style.NumberFormat.Format = "@";
                control.Cell(r, 1).Value = fila.ccb;
                control.Cell(r, 2).Value = fila.ccb_desc;
                Importe(control.Cell(r, 3), fila.importe); Importe(control.Cell(r, 4), fila.importe_iva);
                r++;
            }
            control.Cell(r, 2).Value = "Total";
            Importe(control.Cell(r, 3), bloque.TotalContabilidad); Importe(control.Cell(r, 4), bloque.TotalLibro);
            Total(control, r++, 4);
            if (bloque.Diferencia != 0)
            {
                control.Cell(r, 2).Value = "Dif. (Contabilidad − Libro IVA)";
                Importe(control.Cell(r, 3), bloque.Diferencia);
                Total(control, r++, 4);
            }
            control.Columns(1, 1).Width = 17; control.Column(2).Width = 43; control.Columns(3, 4).Width = 27;
            Acabado(control, r - 1, 4);

            var resumen = wb.AddWorksheet($"Resumen {sufijo}");
            Encabezado(resumen, libro, bloque.Nombre, 2);
            resumen.Cell(4, 1).Value = "Concepto"; resumen.Cell(4, 2).Value = "Importe";
            r = 5;
            foreach (var fila in bloque.Resumen)
            {
                ct.ThrowIfCancellationRequested();
                resumen.Cell(r, 1).Value = fila.tipo_desc;
                Importe(resumen.Cell(r, 2), fila.importe); r++;
            }
            resumen.Cell(r, 1).Value = "Total"; Importe(resumen.Cell(r, 2), bloque.TotalResumen);
            Total(resumen, r, 2);
            resumen.Column(1).Width = 48; resumen.Column(2).Width = 29;
            Acabado(resumen, r, 2);
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void Encabezado(IXLWorksheet ws, IvaComprasResumenLibro libro, string nombre, int columnas)
    {
        ws.Range(1, 1, 1, columnas).Merge().FirstCell().Value = $"Libro IVA Compras - Resumen | {libro.Filtro.PeriodoTexto}";
        ws.Range(2, 1, 2, columnas).Merge().FirstCell().Value = nombre;
        ws.Range(3, 1, 3, columnas).Merge().FirstCell().Value = "Importes en pesos · Datos completos de los SP";
        ws.Row(1).Style.Font.Bold = true; ws.Row(1).Style.Font.FontSize = 15;
        ws.Row(1).Height = 26; ws.Row(2).Height = 22;
    }
    private static void Importe(IXLCell cell, decimal valor)
    {
        // Excel conserva sólo 15 cifras significativas: no perder centavos de grandes totales.
        if (IvaVentasResumenReporte.ExcelComoTexto(valor))
        {
            cell.Style.NumberFormat.Format = "@";
            cell.Value = valor.ToString("F2", CultureInfo.GetCultureInfo("es-AR"));
        }
        else { cell.Value = valor; cell.Style.NumberFormat.Format = "#,##0.00;[Red]-#,##0.00"; }
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }
    private static void Total(IXLWorksheet ws, int fila, int columnas)
    {
        var range = ws.Range(fila, 1, fila, columnas);
        range.Style.Font.Bold = true; range.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8ECC4");
        range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
    }
    private static void Acabado(IXLWorksheet ws, int ultima, int columnas)
    {
        ws.ShowGridLines = false;
        ws.Style.Font.FontName = "Calibri"; ws.Style.Font.FontSize = 11;
        ws.Range(4, 1, 4, columnas).Style.Fill.BackgroundColor = XLColor.FromHtml("#926E0B");
        ws.Range(4, 1, 4, columnas).Style.Font.FontColor = XLColor.White;
        ws.Range(4, 1, 4, columnas).Style.Font.Bold = true;
        ws.Range(4, 1, ultima, columnas).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Rows(4, ultima).Height = 21;
        ws.SheetView.FreezeRows(4);
        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0);
        ws.PageSetup.SetRowsToRepeatAtTop(1, 4);
        ws.PageSetup.PrintAreas.Add(ws.Range(1, 1, ultima, columnas).RangeAddress.ToString());
    }

    public static byte[] Pdf(IvaComprasResumenLibro libro, EmpresaGeco empresa, string? logo = null, CancellationToken ct = default)
    {
        libro.Validar();
        if (libro.Cantidad == 0) throw new ArgumentException("No hay filas para imprimir.");
        using var ms = new MemoryStream();
        using var doc = new Document(PageSize.A4.Rotate(), 22, 22, 79, 30);
        var writer = PdfWriter.GetInstance(doc, ms);
        writer.PageEvent = new Cabecera(libro.Filtro.PeriodoTexto, empresa, logo);
        doc.Open();
        foreach (var bloque in libro.Bloques)
        {
            ct.ThrowIfCancellationRequested();
            // Dos tablas contiguas por bloque, sin recortar filas si una consulta crece.
            var pareja = new PdfPTable(2) { WidthPercentage = 100, SplitLate = false, SplitRows = true, SpacingAfter = 10 };
            pareja.SetWidths(new float[] { 60, 40 });
            var control = Tabla(bloque, true);
            foreach (var fila in bloque.Control)
            {
                ct.ThrowIfCancellationRequested();
                control.AddCell(Texto(fila.Concepto)); control.AddCell(Numero(fila.importe)); control.AddCell(Numero(fila.importe_iva));
            }
            control.AddCell(Texto("Total", true)); control.AddCell(Numero(bloque.TotalContabilidad, true)); control.AddCell(Numero(bloque.TotalLibro, true));
            if (bloque.Diferencia != 0)
            {
                var dif = Texto($"Dif.: {IvaComprasResumenLibro.Numero(bloque.Diferencia)}   (Contabilidad - Libro IVA)", true);
                dif.Colspan = 3; dif.BackgroundColor = new BaseColor(255, 243, 228); control.AddCell(dif);
            }
            var resumen = Tabla(bloque, false);
            foreach (var fila in bloque.Resumen)
            {
                ct.ThrowIfCancellationRequested();
                resumen.AddCell(Texto(fila.tipo_desc)); resumen.AddCell(Numero(fila.importe));
            }
            resumen.AddCell(Texto("Total", true)); resumen.AddCell(Numero(bloque.TotalResumen, true));
            pareja.AddCell(new PdfPCell(control) { Border = Rectangle.NO_BORDER, PaddingRight = 10 });
            pareja.AddCell(new PdfPCell(resumen) { Border = Rectangle.NO_BORDER });
            doc.Add(pareja);
        }
        doc.Close();
        return ms.ToArray();
    }

    private static Font Fuente(bool bold = false, bool blanca = false) => new(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8.5f,
        bold ? Font.BOLD : Font.NORMAL, blanca ? new BaseColor(255, 255, 255) : new BaseColor(54, 49, 40));
    private static PdfPCell Texto(string valor, bool bold = false) => new(new Phrase(valor, Fuente(bold)))
        { Padding = 3, BorderColor = new BaseColor(228, 217, 183), BackgroundColor = bold ? new BaseColor(248, 236, 196) : new BaseColor(255, 255, 255) };
    private static PdfPCell Numero(decimal valor, bool bold = false)
    {
        var cell = Texto(IvaComprasResumenLibro.Numero(valor), bold);
        cell.HorizontalAlignment = Element.ALIGN_RIGHT; cell.NoWrap = true;
        return cell;
    }
    private static PdfPTable Tabla(IvaComprasResumenBloque bloque, bool control)
    {
        var n = control ? 3 : 2;
        var tabla = new PdfPTable(n) { WidthPercentage = 100, HeaderRows = 2, SplitLate = false };
        tabla.SetWidths(control ? new float[] { 46, 27, 27 } : new float[] { 59, 41 });
        var titulo = Texto($"{(control ? "Control contable" : "Resumen impositivo")} - {bloque.Nombre}", true);
        titulo.Colspan = n; titulo.Padding = 5; tabla.AddCell(titulo);
        foreach (var col in control ? new[] { "Concepto", "Contabilidad", "Libro IVA" } : new[] { "Concepto", "Importe" })
            tabla.AddCell(new PdfPCell(new Phrase(col, Fuente(true, true))) { Padding = 4, BackgroundColor = new BaseColor(146, 110, 11), BorderColor = new BaseColor(146, 110, 11) });
        return tabla;
    }
    private sealed class Cabecera(string periodo, EmpresaGeco empresa, string? logo) : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var tabla = new PdfPTable(2) { TotalWidth = document.PageSize.Width - 44 };
            tabla.SetWidths(new float[] { 48, 52 });
            var compania = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 0 };
            if (!string.IsNullOrWhiteSpace(logo) && File.Exists(logo))
            {
                var imagen = Image.GetInstance(logo); imagen.ScaleToFit(30, 20); compania.AddElement(imagen);
            }
            compania.AddElement(new Phrase(string.IsNullOrWhiteSpace(empresa.RazonSocial) ? empresa.Nombre : empresa.RazonSocial, Fuente(true)));
            compania.AddElement(new Phrase($"CUIT: {empresa.CUIT} · IB: {empresa.IngresosBrutos}", Fuente()));
            compania.AddElement(new Phrase($"{empresa.Direccion} · {empresa.Localidad}", Fuente()));
            tabla.AddCell(compania);
            var titulo = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 0 };
            titulo.AddElement(new Paragraph($"Libro IVA Compras - Resumen {periodo}", new Font(Fuente(true).BaseFont, 13, Font.BOLD)));
            titulo.AddElement(new Paragraph("Control contable y resumen impositivo · Importes en pesos", Fuente()));
            tabla.AddCell(titulo);
            tabla.WriteSelectedRows(0, -1, 22, document.PageSize.Height - 12, writer.DirectContent);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase($"Emitido: {IvaVentasFiltro.HoyArgentina():dd/MM/yyyy}", Fuente()), 22, 15, 0);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_RIGHT, new Phrase($"Página {writer.PageNumber}", Fuente()), document.PageSize.Width - 22, 15, 0);
        }
    }
}
