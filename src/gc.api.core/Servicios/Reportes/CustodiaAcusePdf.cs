using System.Globalization;
using gc.infraestructura.Dtos.Cajas;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace gc.api.core.Servicios.Reportes;

// Se genera sobre la base local de Caja que registró la entrega, mediante SPGECO_ENT_Datos.
public static class CustodiaAcusePdf
{
    private static readonly BaseColor Dorado = new(187, 143, 13);
    private static readonly Font Normal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
    private static readonly Font Negrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
    private static string Moneda(string ins) => ins.Trim() switch { "PES" => "$", "DOL" => "U$S", var otra => otra };
    private static string Importe(decimal valor, string ins) => Moneda(ins) + " " + valor.ToString("N2", Cultura);

    public static byte[] Generar(IReadOnlyList<CustodiaAcuseFilaDto> filas)
    {
        if (filas.Count == 0) throw new ArgumentException("No hay datos para generar el acuse.");
        var cabecera = filas[0];
        if (filas.Any(f => f.ent_compte != cabecera.ent_compte || f.adm_id != cabecera.adm_id))
            throw new ArgumentException("Los datos del acuse no corresponden a una única entrega y sucursal.");
        using var stream = new MemoryStream();
        using (var pdf = new Document(PageSize.A4.Rotate(), 28, 28, 30, 35))
        {
            var writer = PdfWriter.GetInstance(pdf, stream); writer.CloseStream = false;
            writer.PageEvent = new Pie();
            pdf.AddTitle("Acuse de entrega en custodia " + cabecera.ent_compte);
            pdf.AddAuthor("GECO Caja"); pdf.Open();
            pdf.Add(new Paragraph("GECO CAJA · ENTREGA DE VALORES EN CUSTODIA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, Dorado)) { SpacingAfter = 12 });
            pdf.Add(new Paragraph("ACUSE · Entrega N.º " + cabecera.ent_compte, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)) { SpacingAfter = 6 });
            pdf.Add(new Paragraph("Sucursal: " + cabecera.adm_id + " · " + cabecera.adm_nombre + "     Fecha: " + cabecera.ent_fecha.ToString("dd/MM/yyyy HH:mm", Cultura), Normal));
            pdf.Add(new Paragraph("Responsable de la entrega: " + cabecera.ent_resp_entrega, Normal));
            pdf.Add(new Paragraph("En custodia de: " + cabecera.ent_resp_recibe, Normal));
            pdf.Add(new Paragraph("Comprobante de entrega: " + cabecera.ent_compte_retiro, Normal) { SpacingAfter = 14 });
            var tabla = new PdfPTable(8) { WidthPercentage = 100, HeaderRows = 1, SpacingAfter = 12 };
            tabla.SetWidths([11f, 18f, 12f, 10f, 7f, 10f, 16f, 16f]);
            foreach (var titulo in new[] { "Fecha", "Proceso", "Cierre / Rend.", "Tipo", "PV", "Moneda", "Rendido", "Importe OK" })
                tabla.AddCell(new PdfPCell(new Phrase(titulo, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.White))) { BackgroundColor = Dorado, Padding = 7, BorderColor = Dorado });
            var indice = 0;
            foreach (var f in filas.OrderBy(f => f.ins_id).ThenBy(f => f.rend_fecha).ThenBy(f => f.caja_nro_proceso).ThenBy(f => f.caja_nro_cierre).ThenBy(f => f.caja_nro_rend).ThenBy(f => f.rend_item))
            {
                var valores = new[] { f.rend_fecha.ToString("dd/MM/yyyy", Cultura), f.caja_nro_proceso, f.caja_nro_cierre + " / " + f.caja_nro_rend, f.rend_tipo == "P" ? "Parcial" : "Final", f.caja_id, f.ins_id.Trim(), Importe(f.rend_importe_arq, f.ins_id), Importe(f.rend_importe_ok, f.ins_id) };
                for (var i = 0; i < valores.Length; i++) tabla.AddCell(new PdfPCell(new Phrase(valores[i] ?? "", Normal)) { Padding = 6, BorderColor = new BaseColor(226, 222, 211), BackgroundColor = indice % 2 == 0 ? BaseColor.White : new BaseColor(251, 248, 240), HorizontalAlignment = i >= 6 ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT });
                indice++;
            }
            pdf.Add(tabla);
            foreach (var grupo in filas.GroupBy(f => f.ins_id.Trim()).OrderBy(g => g.Key == "PES" ? 0 : 1))
                pdf.Add(new Paragraph("TOTAL " + grupo.Key + ": " + Importe(grupo.Sum(f => f.rend_importe_ok), grupo.Key), Negrita) { Alignment = Element.ALIGN_RIGHT, SpacingAfter = 5 });
            pdf.Add(new Paragraph(" ") { SpacingAfter = 15 });
            var firmas = new PdfPTable(2) { WidthPercentage = 100 };
            foreach (var responsable in new[] { "Entrega: " + cabecera.ent_resp_entrega, "Recibe en custodia: " + cabecera.ent_resp_recibe })
                firmas.AddCell(new PdfPCell(new Phrase("______________________________\n\n" + responsable, Normal)) { Border = Rectangle.NO_BORDER, PaddingTop = 16, HorizontalAlignment = Element.ALIGN_CENTER });
            pdf.Add(firmas); pdf.Close();
        }
        return stream.ToArray();
    }
    private sealed class Pie : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_LEFT, new Phrase("GECO Caja · Acuse de entrega de valores en custodia", FontFactory.GetFont(FontFactory.HELVETICA, 8)), document.LeftMargin, 18, 0);
            ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_RIGHT, new Phrase("Página " + writer.PageNumber, Normal), document.PageSize.Width - document.RightMargin, 18, 0);
        }
    }
}
