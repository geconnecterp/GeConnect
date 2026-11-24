using System.Globalization;
using System.Text.RegularExpressions;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Cajas.Response;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Reportes;

// Representación del comprobante ya registrado. No calcula impuestos ni confirma operaciones.
public static class FacturaBTicketPdf
{
    public const float AnchoMm = 80f;
    public const int CaracteresDescripcion = 44;
    private const float Mm = 72f / 25.4f;
    private const float Margen = 4f * Mm;
    private const float Ancho = AnchoMm * Mm;
    private const float Util = Ancho - 2 * Margen;
    // Limita cada página PDF a menos de 200 pulgadas; no recorta productos extensos.
    private const float AlturaMaxima = 14000f;
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
    private static readonly Font Normal = FontFactory.GetFont(FontFactory.COURIER, 7.5f);
    private static readonly Font Negrita = FontFactory.GetFont(FontFactory.COURIER_BOLD, 7.5f);
    private static readonly Font Titulo = FontFactory.GetFont(FontFactory.COURIER_BOLD, 10f);
    private static string Importe(decimal n) => n.ToString("N2", Cultura);
    private static string Texto(string? s) => Regex.Replace(s?.Trim() ?? "", @"\s+", " ");

    public static byte[] Generar(FeResDto cabecera, IReadOnlyList<FeDetResDto> productos,
        IReadOnlyList<FePerResDto> percepciones, string? logoPath, string leyenda = "ORIGINAL", ILogger? logger = null)
    {
        if (!string.Equals(cabecera.tco_letra?.Trim(), "B", StringComparison.OrdinalIgnoreCase))
            throw new NegocioException("El ticket de 80 mm está disponible para comprobantes B.");
        if (productos.Count == 0) throw new NegocioException("No hay productos para generar el ticket.");
        bool Mismo(string? tipo, string? numero, string? repetido) =>
            (string.IsNullOrWhiteSpace(tipo) || tipo.Trim() == cabecera.tco_id.Trim()) &&
            (string.IsNullOrWhiteSpace(numero) || numero.Trim() == cabecera.cm_compte.Trim()) &&
            (string.IsNullOrWhiteSpace(repetido) || repetido.Trim() == cabecera.cm_repetido.Trim());
        if (productos.Any(p => !Mismo(p.tco_id, p.cm_compte, p.cm_repetido)) ||
            percepciones.Any(p => !Mismo(p.tco_id, p.cm_compte, p.cm_repetido)))
            throw new NegocioException("Los datos del ticket corresponden a comprobantes diferentes.");
        // El generador compartido conserva compatibilidad histórica; el ticket exige autorización real.
        if (!long.TryParse(cabecera.cm_cae?.Trim(), out var cae) || cae <= 0 ||
            cabecera.cm_cae_vto is null || cabecera.cm_cae_vto.Value.Year <= 1900)
            throw new NegocioException("El comprobante no informa CAE y vencimiento válidos. Revise los datos antes de imprimir el ticket.");
        var qr = FacturaBase.GenerarQrArca(cabecera, logger)
            ?? throw new NegocioException("No se pudo generar el QR del comprobante. Revise los datos fiscales.");
        if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            throw new NegocioException("No se encontró el logo configurado para el ticket.");
        var logo = Image.GetInstance(logoPath);
        logo.ScaleToFit(28 * Mm, 18 * Mm);
        qr.ScaleAbsolute(35 * Mm, 35 * Mm);

        var ordenados = productos.Select((p,i) => (p,i))
            .OrderBy(x => int.TryParse(x.p.cmd_item, out var n) ? n : int.MaxValue)
            .ThenBy(x => x.i).Select(x => Producto(x.p)).ToList();
        var pie = Pie(cabecera, percepciones, qr);
        var paginas = new List<List<PdfPTable>>();
        var actual = new List<PdfPTable> { Cabecera(cabecera, logo, leyenda, false) };
        var alto = actual.Sum(t => t.TotalHeight);
        for (var i = 0; i < ordenados.Count; i++)
        {
            var bloque = ordenados[i];
            var reserva = i == ordenados.Count - 1 ? pie.TotalHeight : 0;
            if (alto + bloque.TotalHeight + reserva + Margen * 2 > AlturaMaxima)
            {
                if (actual.Count == 1) throw new NegocioException("Un artículo supera el alto admitido por el formato de ticket.");
                paginas.Add(actual);
                actual = new() { Cabecera(cabecera, logo, leyenda, true) };
                alto = actual[0].TotalHeight;
            }
            if (alto + bloque.TotalHeight + reserva + Margen * 2 > AlturaMaxima)
                throw new NegocioException("El contenido supera el alto admitido por el formato de ticket.");
            actual.Add(bloque); alto += bloque.TotalHeight;
        }
        actual.Add(pie); paginas.Add(actual);
        using var stream = new MemoryStream();
        using (var pdf = new Document(new Rectangle(Ancho, Alto(paginas[0])), Margen, Margen, Margen, Margen))
        {
            var writer = PdfWriter.GetInstance(pdf, stream); writer.CloseStream = false;
            pdf.AddTitle("Comprobante B " + cabecera.cm_compte); pdf.AddAuthor("GECO Caja"); pdf.Open();
            for (var i = 0; i < paginas.Count; i++)
            {
                if (i > 0) { pdf.SetPageSize(new Rectangle(Ancho, Alto(paginas[i]))); pdf.NewPage(); }
                var y = pdf.PageSize.Height - Margen;
                foreach (var bloque in paginas[i]) { bloque.WriteSelectedRows(0, -1, Margen, y, writer.DirectContent); y -= bloque.TotalHeight; }
            }
            pdf.Close();
        }
        logger?.LogInformation("[TICKET_B] Comprobante={Comprobante} AnchoMm={Ancho} Productos={Productos} Paginas={Paginas} CaracteresDescripcion={Caracteres}",
            cabecera.cm_compte, AnchoMm, productos.Count, paginas.Count, CaracteresDescripcion);
        return stream.ToArray();
    }

    private static float Alto(List<PdfPTable> pagina) => MathF.Ceiling(pagina.Sum(t => t.TotalHeight) + 2 * Margen + 1);
    private static PdfPTable Tabla(int columnas = 1) => new(columnas) { TotalWidth = Util, LockedWidth = true, WidthPercentage = 100 };
    private static PdfPCell Celda(string s, Font? fuente = null, int alineacion = Element.ALIGN_LEFT) => new(new Phrase(s, fuente ?? Normal))
    { Border = Rectangle.NO_BORDER, PaddingLeft = 0, PaddingRight = 0, PaddingTop = 1, PaddingBottom = 1,
        HorizontalAlignment = alineacion, VerticalAlignment = Element.ALIGN_TOP, UseAscender = true, UseDescender = true };
    private static void Linea(PdfPTable t, string s, Font? fuente = null, int alineacion = Element.ALIGN_LEFT)
    {
        foreach (var renglon in Envolver(s)) t.AddCell(Celda(renglon, fuente, alineacion));
    }
    private static IEnumerable<string> Envolver(string? valor)
    {
        var texto = Texto(valor);
        while (texto.Length > CaracteresDescripcion)
        {
            var corte = texto.LastIndexOf(' ', CaracteresDescripcion);
            if (corte <= 0) corte = CaracteresDescripcion;
            yield return texto[..corte]; texto = texto[corte..].TrimStart();
        }
        yield return texto;
    }
    private static void Separador(PdfPTable t) => t.AddCell(new PdfPCell { Border = Rectangle.BOTTOM_BORDER, BorderWidthBottom = .4f, FixedHeight = 5, Padding = 0 });
    private static void Bloque(PdfPTable t, PdfPTable contenido) => t.AddCell(new PdfPCell(contenido) { Border = Rectangle.NO_BORDER, Padding = 0 });
    private static void Par(PdfPTable t, string etiqueta, string valor, Font? fuente = null)
    {
        // Un importe excepcionalmente largo conserva todos sus dígitos en una línea completa.
        if (valor.Length * (fuente ?? Normal).Size * .6f > Util * .48f)
        {
            Linea(t, etiqueta, fuente); Linea(t, valor, fuente, Element.ALIGN_RIGHT); return;
        }
        var fila = Tabla(2); fila.SetWidths(new[] { 52f, 48f });
        fila.AddCell(Celda(etiqueta, fuente)); fila.AddCell(Celda(valor, fuente, Element.ALIGN_RIGHT));
        Bloque(t, fila);
    }
    private static PdfPTable Cabecera(FeResDto c, Image logo, string leyenda, bool continuacion)
    {
        var t = Tabla();
        t.AddCell(new PdfPCell(Image.GetInstance(logo), false) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 0, PaddingBottom = 5 });
        Linea(t, c.emp_razon_social, Negrita, Element.ALIGN_CENTER);
        Linea(t, "CUIT: " + Texto(c.emp_cuit)); Linea(t, "Ing. Brutos: " + Texto(c.emp_ib_nro));
        Linea(t, FacturaBase.ObtenerDomicilioEmpresa(c));
        if (c.emp_inicio_act.Year > 1900) Linea(t, "Inicio de actividades: " + c.emp_inicio_act.ToString("dd/MM/yyyy"));
        Linea(t, c.afip_desc_emp); Separador(t);
        // El tipo fiscal es el recibido del SP, independiente del soporte de papel.
        Linea(t, Texto(c.tco_desc), Negrita, Element.ALIGN_CENTER);
        Linea(t, "Código " + c.tco_id + " - Letra B", Negrita, Element.ALIGN_CENTER);
        Linea(t, leyenda, Negrita, Element.ALIGN_CENTER);
        if (continuacion) Linea(t, "CONTINUACION", Negrita, Element.ALIGN_CENTER);
        Linea(t, "N° " + c.cm_compte, Negrita, Element.ALIGN_CENTER);
        Linea(t, "Fecha: " + c.cm_fecha.ToString("dd/MM/yyyy HH:mm")); Separador(t);
        Linea(t, c.cm_nombre, Negrita); Linea(t, "Documento: " + Texto(c.cm_cuit));
        Linea(t, c.afip_desc_cli); Linea(t, c.cm_domicilio);
        if (!string.IsNullOrWhiteSpace(c.compte_ori)) Linea(t, "Origen: " + c.compte_ori);
        Separador(t);
        Par(t, "Cant./Precio Unit.", "Código", Negrita);
        Par(t, "Descripción / IVA / II", "Importe", Negrita);
        return t;
    }
    private static PdfPTable Producto(FeDetResDto p)
    {
        var t = Tabla();
        Par(t, p.cmd_cantidad.ToString("0.000", Cultura) + " x " + Importe(p.cmd_pvta), Texto(p.p_id));
        Linea(t, p.p_desc);
        var tasas = "IVA " + p.iva_alicuota.ToString("0.##", Cultura) + "%";
        if (p.in_alicuota != 0) tasas += " II " + p.in_alicuota.ToString("0.##", Cultura) + "%";
        Par(t, tasas, Importe(p.cmd_subtotal_con_iva), Negrita);
        if (p.cmd_boni != 0) Par(t, "Bonif. incluida", Importe(p.cmd_boni));
        t.AddCell(new PdfPCell { Border = Rectangle.NO_BORDER, FixedHeight = 3 });
        return t;
    }
    private static PdfPTable Pie(FeResDto c, IReadOnlyList<FePerResDto> percepciones, Image qr)
    {
        var t = Tabla(); Separador(t);
        if (c.cm_dto != 0) Par(t, "Rec./Dto.", Importe(c.cm_dto));
        // II ya forma parte de los precios de B; se informa sin sumarlo nuevamente.
        foreach (var p in percepciones.Where(p => !string.Equals(p.imp_id?.Trim(), "II", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.orden).ThenBy(p => p.imp_id))
        {
            Linea(t, p.imp_des, Negrita);
            Par(t, "Base " + Importe(p.Base ?? 0) + " / " + (p.ali ?? 0).ToString("0.##", Cultura) + "%", Importe(p.percepcion ?? 0));
        }
        if (c.cm_percepciones != 0 && !percepciones.Any(p => p.imp_id?.Trim() != "II")) Par(t, "Percepciones", Importe(c.cm_percepciones));
        Par(t, "TOTAL " + Texto(c.mon_codigo), Importe(c.cm_total), Titulo);
        Separador(t); Linea(t, "Transparencia fiscal", Negrita);
        Par(t, "IVA contenido", Importe(c.cm_iva)); Par(t, "Impuestos internos", Importe(c.cm_ii));
        if (!string.IsNullOrWhiteSpace(c.usu_id)) Linea(t, "Cajero: " + Texto(c.usu_id));
        if (!string.IsNullOrWhiteSpace(c.sorteo)) Linea(t, "Sorteos: " + c.sorteo);
        Separador(t);
        Linea(t, "CAE: " + c.cm_cae, Negrita);
        Linea(t, "Vencimiento CAE: " + c.cm_cae_vto!.Value.ToString("dd/MM/yyyy"));
        t.AddCell(new PdfPCell(Image.GetInstance(qr), false) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 4, PaddingBottom = 4 });
        Linea(t, "Comprobante electrónico autorizado", Normal, Element.ALIGN_CENTER);
        return t;
    }
}
