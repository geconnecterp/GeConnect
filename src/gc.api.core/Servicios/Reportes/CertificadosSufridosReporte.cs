using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
using static gc.infraestructura.Dtos.Libros.CertificadosSufridosConsulta;

namespace gc.api.core.Servicios.Reportes;

public static class CertificadosSufridosReporte
{
    public static byte[] Pdf(CertificadosSufridosConsulta datos, CertificadosSufridosFiltro filtro, EmpresaGeco empresa,
        string? logo = null, CancellationToken ct = default)
    {
        datos.ValidarModo(filtro);
        if (filtro.AgrupadoPorCuit) return PdfPorCuit(datos, filtro, empresa, logo, ct);
        if (datos.Filas.Count == 0) throw new ArgumentException("No hay registros para imprimir con estos filtros.");
        var ib = filtro.Impuesto == "IB";
        using var ms = new MemoryStream();
        using var doc = new Document(PageSize.A4.Rotate(), 22, 22, 110, 32);
        var writer = PdfWriter.GetInstance(doc, ms); writer.CloseStream = false;
        writer.PageEvent = new Cabecera(filtro, datos.ImpuestoTexto, empresa, logo);
        doc.Open();
        var normal = FontFactory.GetFont(FontFactory.HELVETICA, 7);
        var fuerte = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7);
        var blanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.White);
        var columnas = new List<string> { "Fecha", "Origen", "Tipo / Comprobante", "Base", "Alíc.", "Importe" };
        var anchos = new List<float> { 46, 47, 103, 74, 28, 68 };
        if (ib) { columnas.AddRange(["Alíc. LH", "Lote Hogar", "Total con LH"]); anchos.AddRange([28, 68, 74]); }
        columnas.AddRange(["Tipo", "Razón social", "CUIT"]); anchos.AddRange([46, 106, 72]);
        if (ib) { columnas.Add("Nro. IB"); anchos.Add(36); }
        var grupos = datos.Grupos(filtro);
        var totales = grupos.Select(g => g.Filas).Append(datos.Filas).ToList();
        void Ancho(int columna, Func<CertificadoSufridoFila, decimal?> campo, bool sumar)
        {
            var valores = datos.Filas.Select(campo).Concat(sumar ? totales.Select(fs => Sumar(fs, campo)) : []);
            anchos[columna] = Math.Max(anchos[columna], valores.Max(v => fuerte.BaseFont.GetWidthPoint(Numero(v), 7)) + 10);
        }
        Ancho(3, f => f.@base, true); Ancho(4, f => f.ali, false); Ancho(5, f => f.percepcion, true);
        if (ib) { Ancho(6, f => f.ali_lh, false); Ancho(7, f => f.percepcion_lh, true); Ancho(8, f => TotalFila(f, true), true); }
        var disponible = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
        foreach (var (columna, minimo) in new[] { (2, 80f), (ib ? 10 : 7, 80f), (ib ? 12 : 8, ib ? 16f : 68f) })
        {
            var reducir = Math.Min(Math.Max(0, anchos.Sum() - disponible), Math.Max(0, anchos[columna] - minimo)); anchos[columna] -= reducir;
        }
        if (anchos.Sum() > disponible) throw new InvalidOperationException("Los importes exceden el ancho legible del reporte. No se generó un PDF con cifras recortadas.");
        foreach (var grupo in grupos)
        {
            ct.ThrowIfCancellationRequested();
            var tabla = new PdfPTable(columnas.Count) { WidthPercentage = 100, HeaderRows = 2, SplitLate = false, SplitRows = false, SpacingAfter = 10 };
            tabla.SetWidths(anchos.ToArray());
            tabla.AddCell(new PdfPCell(new Phrase(grupo.Titulo + $" · {grupo.Filas.Count} registros", fuerte))
            { Colspan = columnas.Count, Padding = 5, BackgroundColor = new BaseColor(248, 236, 196) });
            foreach (var columna in columnas) tabla.AddCell(new PdfPCell(new Phrase(columna, blanco))
            { Padding = 4, BackgroundColor = new BaseColor(146, 110, 11) });
            foreach (var f in grupo.Filas)
            {
                ct.ThrowIfCancellationRequested();
                var textos = new List<string> { f.cm_fecha?.ToString("dd/MM/yyyy") ?? "—", Origen(f.origen), Texto(f.tco_desc) + " " + Texto(f.cm_compte),
                    Numero(f.@base), Numero(f.ali), Numero(f.percepcion) };
                if (ib) textos.AddRange([Numero(f.ali_lh), Numero(f.percepcion_lh), Numero(TotalFila(f, true))]);
                textos.AddRange([Texto(f.tipo_desc), Texto(f.cm_nombre), Texto(f.cm_cuit)]);
                if (ib) textos.Add("—"); // El SP entregado no devuelve ib_nro.
                for (var c = 0; c < textos.Count; c++) tabla.AddCell(new PdfPCell(new Phrase(textos[c], normal))
                { Padding = 3, NoWrap = c == 0 || (c >= 3 && c < (ib ? 9 : 6)), HorizontalAlignment = c >= 3 && c < (ib ? 9 : 6) ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT });
            }
            Totales(tabla, grupo.Filas, ib, columnas.Count, fuerte, "Subtotal del grupo");
            doc.Add(tabla);
        }
        var total = new PdfPTable(columnas.Count) { WidthPercentage = 100, SplitRows = false };
        total.SetWidths(anchos.ToArray());
        Totales(total, datos.Filas, ib, columnas.Count, fuerte, "TOTAL GENERAL"); doc.Add(total);
        doc.Add(new Paragraph("Alícuotas con la escala original del SP, sin conversión. " + (ib ? "Inscripción IB del agente no informada por el SP. " : "") +
            "Reporte completo según los filtros; los datos se consultan nuevamente al imprimir.", normal));
        doc.Close(); return ms.ToArray();
    }
    private static byte[] PdfPorCuit(CertificadosSufridosConsulta datos, CertificadosSufridosFiltro filtro,
        EmpresaGeco empresa, string? logo, CancellationToken ct)
    {
        if (datos.FilasPorCuit.Count == 0) throw new ArgumentException("No hay registros agrupados para imprimir con estos filtros.");
        using var ms = new MemoryStream();
        using var doc = new Document(PageSize.A4.Rotate(), 22, 22, 110, 32);
        var writer = PdfWriter.GetInstance(doc, ms); writer.CloseStream = false;
        writer.PageEvent = new Cabecera(filtro, datos.ImpuestoTexto, empresa, logo); doc.Open();
        var normal = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        var fuerte = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
        var blanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.White);
        string[] columnas = ["CUIT", "Razón social", "Domicilio", "Impuesto", "Tipo", "Descripción", "Registros de origen", "Total sufrido (incluye LH)"];
        float[] anchos = [78, 170, 155, 50, 36, 80, 75, 110];
        var grupos = datos.GruposPorCuit();
        var conjuntos = grupos.Select(g => g.Filas).Append(datos.FilasPorCuit).ToList();
        anchos[6] = Math.Max(anchos[6], conjuntos.Select(fs => Cantidad(CantidadPorCuit(fs)))
            .Concat(datos.FilasPorCuit.Select(f => Cantidad(f.percepcion_cant))).Max(n => fuerte.BaseFont.GetWidthPoint(n, 8)) + 12);
        anchos[7] = Math.Max(anchos[7], conjuntos.Select(fs => Numero(SumarPorCuit(fs)))
            .Concat(datos.FilasPorCuit.Select(f => Numero(f.percepcion))).Max(n => fuerte.BaseFont.GetWidthPoint(n, 8)) + 12);
        var disponible = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
        foreach (var c in new[] { 2, 1 }) anchos[c] -= Math.Min(Math.Max(0, anchos.Sum() - disponible), Math.Max(0, anchos[c] - 90));
        if (anchos.Sum() > disponible) throw new InvalidOperationException("Los importes agrupados exceden el ancho legible del reporte.");
        void TotalesPorCuit(PdfPTable tabla, List<CertificadoSufridoPorCuitFila> filas, string texto)
        {
            tabla.AddCell(new PdfPCell(new Phrase(texto, fuerte)) { Colspan = 6, Padding = 5, BackgroundColor = new BaseColor(248, 236, 196) });
            foreach (var n in new[] { Cantidad(CantidadPorCuit(filas)), Numero(SumarPorCuit(filas)) })
                tabla.AddCell(new PdfPCell(new Phrase(n, fuerte)) { Padding = 5, NoWrap = true, HorizontalAlignment = Element.ALIGN_RIGHT, BackgroundColor = new BaseColor(248, 236, 196) });
        }
        foreach (var grupo in grupos)
        {
            ct.ThrowIfCancellationRequested();
            var tabla = new PdfPTable(8) { WidthPercentage = 100, HeaderRows = 2, SplitLate = false, SplitRows = false, SpacingAfter = 10 };
            tabla.SetWidths(anchos);
            tabla.AddCell(new PdfPCell(new Phrase(grupo.Titulo + $" · {grupo.Filas.Count} filas agrupadas", fuerte))
            { Colspan = 8, Padding = 5, BackgroundColor = new BaseColor(248, 236, 196) });
            foreach (var col in columnas) tabla.AddCell(new PdfPCell(new Phrase(col, blanco)) { Padding = 5, BackgroundColor = new BaseColor(146, 110, 11) });
            foreach (var f in grupo.Filas)
            {
                ct.ThrowIfCancellationRequested();
                string[] textos = [Texto(f.cm_cuit), Texto(f.cm_nombre), Texto(f.cm_domicilio), Texto(f.imp_id), Texto(f.tipo), Texto(f.tipo_desc), Cantidad(f.percepcion_cant), Numero(f.percepcion)];
                for (var c = 0; c < textos.Length; c++) tabla.AddCell(new PdfPCell(new Phrase(textos[c], normal))
                { Padding = 4, NoWrap = c >= 6, HorizontalAlignment = c >= 6 ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT });
            }
            TotalesPorCuit(tabla, grupo.Filas, "Subtotal del grupo"); doc.Add(tabla);
        }
        var total = new PdfPTable(8) { WidthPercentage = 100, SplitRows = false }; total.SetWidths(anchos);
        TotalesPorCuit(total, datos.FilasPorCuit, "TOTAL GENERAL"); doc.Add(total);
        doc.Add(new Paragraph("El importe agrupado ya incluye Lote Hogar. Registros de origen es la cantidad informada por el SP. Base imponible, alícuotas y corte de ventas no informados en esta vista.", normal));
        doc.Close(); return ms.ToArray();
    }
    private static void Totales(PdfPTable tabla, List<CertificadoSufridoFila> filas, bool ib, int columnas, Font fuente, string etiqueta)
    {
        void Cell(string texto, int span = 1, bool numero = false) => tabla.AddCell(new PdfPCell(new Phrase(texto, fuente))
        { Colspan = span, Padding = 5, BackgroundColor = new BaseColor(248, 236, 196), HorizontalAlignment = numero ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT });
        Cell(etiqueta, 3); Cell(Numero(Sumar(filas, f => f.@base)), numero: true); Cell(""); Cell(Numero(Sumar(filas, f => f.percepcion)), numero: true);
        if (ib) { Cell(""); Cell(Numero(Sumar(filas, f => f.percepcion_lh)), numero: true); Cell(Numero(Sumar(filas, f => TotalFila(f, true))), numero: true); }
        Cell("", columnas - (ib ? 9 : 6));
    }
    private sealed class Cabecera(CertificadosSufridosFiltro filtro, string impuesto, EmpresaGeco empresa, string? logo) : PdfPageEventHelper
    {
        private PdfTemplate? total;
        private readonly DateTime emitido = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Argentina Standard Time");
        public override void OnOpenDocument(PdfWriter writer, Document document) => total = writer.DirectContent.CreateTemplate(32, 12);
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var titulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
            var table = new PdfPTable(2) { TotalWidth = document.PageSize.Width - 44 }; table.SetWidths(new float[] { 43, 57 });
            var company = new PdfPCell { Padding = 5, BorderWidth = 1 };
            if (!string.IsNullOrWhiteSpace(logo) && File.Exists(logo))
            {
                var image = Image.GetInstance(logo); image.ScaleToFit(36, 24); company.AddElement(image);
            }
            company.AddElement(new Phrase(string.IsNullOrWhiteSpace(empresa.RazonSocial) ? empresa.Nombre : empresa.RazonSocial, normal));
            company.AddElement(new Paragraph($"{empresa.Responsabilidad}  Inicio: {(empresa.InicioActividades == default ? "—" : empresa.InicioActividades.ToString("dd/MM/yyyy"))}", normal));
            company.AddElement(new Paragraph($"CUIT: {empresa.CUIT}   IB: {empresa.IngresosBrutos}", normal));
            company.AddElement(new Paragraph($"{empresa.Direccion}, {empresa.Localidad}", normal)); table.AddCell(company);
            var heading = new PdfPCell { Padding = 5, BorderWidth = 1 };
            heading.AddElement(new Paragraph("Percepciones y Retenciones Sufridas", titulo));
            heading.AddElement(new Paragraph($"{impuesto} ({filtro.Impuesto}) · Período {filtro.PeriodoTexto}", normal));
            if (filtro.AgrupadoPorCuit) heading.AddElement(new Paragraph("Agrupado por CUIT · Importes con Lote Hogar", normal));
            heading.AddElement(new Paragraph(filtro.Descripcion, normal)); table.AddCell(heading);
            table.WriteSelectedRows(0, -1, 22, document.PageSize.Height - 12, writer.DirectContent);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase($"Fecha de impresión: {emitido:dd/MM/yyyy HH:mm:ss}", normal), 22, 16, 0);
            var texto = $"Página {writer.PageNumber} de "; var x = document.PageSize.Width - 110;
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase(texto, normal), x, 16, 0);
            writer.DirectContent.AddTemplate(total, x + normal.BaseFont.GetWidthPoint(texto, 8), 16);
        }
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            total!.BeginText(); total.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8);
            total.SetTextMatrix(0, 0); total.ShowText((writer.PageNumber - 1).ToString()); total.EndText();
        }
    }
}
