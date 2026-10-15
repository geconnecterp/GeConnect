using ClosedXML.Excel;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes;

public static class IvaVentasResumenReporte
{
    public const int Id = 103;
    public const string Modulo = "IVA_VENTAS_RESUMEN";
    public const string MimeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public static bool ExcelComoTexto(decimal valor) => Math.Abs(valor) >= 10_000_000_000_000m;

    public static byte[] Excel(IvaVentasResumenLibro datos, CancellationToken ct = default)
    {
        if (datos.Cantidad == 0) throw new ArgumentException("No hay filas para exportar.");
        using var wb = new XLWorkbook();
        // Una hoja por sucursal: cada grupo y Total Libro permanecen completos y separados.
        foreach (var grupo in datos.Grupos)
        {
            ct.ThrowIfCancellationRequested();
            if (grupo.Filas.Count > 1_048_560) throw new InvalidOperationException("El grupo excede la capacidad de una hoja Excel. No se generó un archivo parcial.");
            var ws = wb.AddWorksheet(grupo.EsTotal ? "Total Libro" : $"Sucursal {grupo.Id}");
            ws.Cell(1, 1).Value = $"Libro IVA Ventas - Resumen | {datos.Filtro.PeriodoTexto}";
            ws.Range(1, 1, 1, 7).Merge();
            ws.Cell(2, 1).Value = grupo.EsTotal ? grupo.Nombre : $"{grupo.Nombre} ({grupo.Id})";
            ws.Range(2, 1, 2, 7).Merge();
            ws.Cell(3, 1).Value = "Importes en pesos. Se conservan signos y conceptos del SP. Consulta nueva al emitir.";
            ws.Range(3, 1, 3, 7).Merge();
            ws.Cell(4, 1).Value = "Importes de 16 o más dígitos contando centavos se guardan como texto exacto para evitar pérdida de precisión en Excel.";
            ws.Range(4, 1, 4, 7).Merge();
            for (var c = 0; c < 7; c++) ws.Cell(6, c + 1).Value = IvaVentasResumenLibro.Columnas[c];
            var row = 7;
            foreach (var fila in grupo.Filas)
            {
                ct.ThrowIfCancellationRequested();
                ws.Cell(row, 1).Value = fila.Concepto;
                EscribirImportes(ws, row, fila.Importes);
                row++;
            }
            ws.Cell(row, 1).Value = grupo.EsTotal ? "Total Libro" : "Total sucursal";
            EscribirImportes(ws, row, grupo.Totales);
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8ECC4");
            ws.Range(6, 1, 6, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#926E0B");
            ws.Range(6, 1, 6, 7).Style.Font.FontColor = XLColor.White;
            ws.Range(6, 1, 6, 7).Style.Font.Bold = true;
            ws.Column(1).Width = 38;
            ws.Columns(2, 7).Width = 27;
            ws.Range(7, 2, row, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.SheetView.FreezeRows(6);
            ws.SheetView.FreezeColumns(1);
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);
        }
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
    private static void EscribirImportes(IXLWorksheet ws, int row, decimal[] valores)
    {
        for (var c = 0; c < valores.Length; c++)
        {
            var cell = ws.Cell(row, c + 2);
            if (ExcelComoTexto(valores[c]))
            {
                cell.Style.NumberFormat.Format = "@";
                cell.Value = valores[c].ToString("F2", CultureInfo.GetCultureInfo("es-AR"));
            }
            else
            {
                cell.Value = valores[c];
                cell.Style.NumberFormat.Format = "#,##0.00;[Red]-#,##0.00";
            }
        }
    }

    public static byte[] Pdf(IvaVentasResumenLibro datos, EmpresaGeco empresa, string? logo = null, CancellationToken ct = default)
    {
        if (datos.Cantidad == 0) throw new ArgumentException("No hay filas para imprimir.");
        using var ms = new MemoryStream();
        using var doc = new Document(PageSize.A4.Rotate(), 24, 24, 94, 34);
        var writer = PdfWriter.GetInstance(doc, ms);
        writer.CloseStream = false;
        writer.PageEvent = new Cabecera(datos.Filtro.PeriodoTexto, empresa, logo);
        doc.Open();
        var bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var bold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        const float size = 9;
        var normal = new Font(bf, size, Font.NORMAL, new BaseColor(35, 42, 50));
        var credito = new Font(bf, size, Font.NORMAL, new BaseColor(155, 36, 36));
        var fuerte = new Font(bold, size);
        var blanco = new Font(bold, size, Font.NORMAL, new BaseColor(255, 255, 255));
        var ancho = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
        var importes = datos.Grupos.SelectMany(g => g.Filas.Select(f => f.Importes).Append(g.Totales)).ToList();
        var widths = new float[7];
        for (var c = 0; c < 6; c++) widths[c + 1] = Math.Max(85,
            importes.Max(v => bold.GetWidthPoint(IvaVentasResumenLibro.Numero(v[c]), size)) + 12);
        widths[0] = ancho - widths.Skip(1).Sum();
        if (widths[0] < 140) throw new InvalidOperationException("Los importes exceden el ancho legible del reporte. No se generó un PDF con cifras recortadas.");
        foreach (var grupo in datos.Grupos)
        {
            ct.ThrowIfCancellationRequested();
            var tabla = new PdfPTable(7) { WidthPercentage = 100, HeaderRows = 2, SplitLate = false, SplitRows = false, SpacingAfter = 14 };
            tabla.SetWidths(widths);
            var titulo = new PdfPCell(new Phrase(grupo.EsTotal ? "Total Libro" : $"Sucursal {grupo.Nombre} ({grupo.Id})", fuerte))
            { Colspan = 7, Padding = 7, BackgroundColor = new BaseColor(248, 236, 196) };
            tabla.AddCell(titulo);
            foreach (var col in IvaVentasResumenLibro.Columnas)
                tabla.AddCell(new PdfPCell(new Phrase(col, blanco)) { Padding = 5, BackgroundColor = new BaseColor(146, 110, 11) });
            foreach (var fila in grupo.Filas)
            {
                ct.ThrowIfCancellationRequested();
                var fuente = fila.tipo_ope == "C" ? credito : normal;
                tabla.AddCell(new PdfPCell(new Phrase(fila.Concepto, fuente)) { Padding = 4 });
                foreach (var importe in fila.Importes) tabla.AddCell(CeldaImporte(importe, fuente));
            }
            tabla.AddCell(new PdfPCell(new Phrase(grupo.EsTotal ? "Total Libro" : "Total sucursal", fuerte)) { Padding = 5, BackgroundColor = new BaseColor(248, 236, 196) });
            foreach (var total in grupo.Totales)
            {
                var cell = CeldaImporte(total, fuerte);
                cell.BackgroundColor = new BaseColor(248, 236, 196);
                tabla.AddCell(cell);
            }
            doc.Add(tabla);
        }
        doc.Close();
        return ms.ToArray();
    }
    private static PdfPCell CeldaImporte(decimal valor, Font font) => new(new Phrase(IvaVentasResumenLibro.Numero(valor), font))
    { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4, NoWrap = true };

    private sealed class Cabecera(string periodo, EmpresaGeco empresa, string? logo) : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            var normal = new Font(bf, 8);
            var titulo = new Font(bf, 14, Font.BOLD);
            var table = new PdfPTable(2) { TotalWidth = document.PageSize.Width - 48 };
            table.SetWidths(new float[] { 45, 55 });
            var company = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 2 };
            if (!string.IsNullOrWhiteSpace(logo) && File.Exists(logo))
            {
                var image = Image.GetInstance(logo);
                image.ScaleToFit(32, 24);
                company.AddElement(image);
            }
            company.AddElement(new Phrase(string.IsNullOrWhiteSpace(empresa.RazonSocial) ? empresa.Nombre : empresa.RazonSocial, normal));
            company.AddElement(new Phrase($"CUIT: {empresa.CUIT}  |  IB: {empresa.IngresosBrutos}", normal));
            company.AddElement(new Phrase($"{empresa.Direccion} - {empresa.Localidad}", normal));
            table.AddCell(company);
            var heading = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 2 };
            heading.AddElement(new Paragraph($"Libro IVA Ventas: {periodo}", titulo));
            heading.AddElement(new Paragraph("Resumen por Sucursales y Condición Fiscal", normal));
            heading.AddElement(new Paragraph("Importes en pesos - Consulta completa", normal));
            table.AddCell(heading);
            table.WriteSelectedRows(0, -1, 24, document.PageSize.Height - 15, writer.DirectContent);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT,
                new Phrase($"Emitido: {IvaVentasFiltro.HoyArgentina():dd/MM/yyyy}", normal), 24, 17, 0);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_RIGHT,
                new Phrase($"Página {writer.PageNumber}", normal), document.PageSize.Width - 24, 17, 0);
        }
    }
}
