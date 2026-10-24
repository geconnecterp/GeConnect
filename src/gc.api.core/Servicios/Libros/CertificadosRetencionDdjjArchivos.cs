using ClosedXML.Excel;
using System.Text;

namespace gc.api.core.Servicios.Libros;

public static class CertificadosRetencionDdjjArchivos
{
    public record Columna(string Campo, string Titulo, Type Tipo, double Ancho);
    // El CUIT repetido de IB se conserva por ordinal; no usar un diccionario por nombre.
    public static Columna[] Columnas(string impuesto) => impuesto switch
    {
        "GA" => [new("", "", typeof(string), 0)],
        "IB" => [new("cib_raz_soc", "Razón social", typeof(string), 44), new("cib_cuit", "CUIT", typeof(string), 18),
            new("cib_domicilio", "Domicilio", typeof(string), 62), new("cib_cuit", "CUIT (2C)", typeof(string), 30),
            new("cib_nro", "Certificado", typeof(string), 24), new("cib_fecha", "Fecha", typeof(DateTime), 14),
            new("cib_base", "Base imponible", typeof(decimal), 25), new("cib_ali", "Alícuota (%)", typeof(decimal), 16), new("cib_reten", "Retención", typeof(decimal), 25)],
        "IV" => [new("civa_nro", "Certificado", typeof(string), 24), new("civa_cuit", "CUIT", typeof(string), 18),
            new("civa_raz_soc", "Razón social", typeof(string), 44), new("civa_domicilio", "Domicilio", typeof(string), 62),
            new("civa_fecha", "Fecha", typeof(DateTime), 14), new("civa_base", "Base imponible", typeof(decimal), 25),
            new("civa_reten", "Retención", typeof(decimal), 25), new("civa_estado", "Estado", typeof(string), 12), new("op_compte", "Comprobante / OP", typeof(string), 24)],
        _ => throw new ArgumentException("Impuesto de DDJJ no soportado.")
    };

    public static async Task Escribir(Stream destino, string impuesto, IAsyncEnumerable<object?[]> filas, CancellationToken ct)
    {
        var columnas = Columnas(impuesto);
        if (impuesto == "GA")
        {
            await using var writer = new StreamWriter(destino, new UTF8Encoding(false, true), 65536, leaveOpen: true) { NewLine = "\r\n" };
            var cantidad = 0;
            await foreach (var fila in filas.WithCancellation(ct))
            {
                ct.ThrowIfCancellationRequested();
                ValidarFila(fila, columnas);
                if (fila[0] is not string linea || linea.Length == 0 || linea.Contains('\r') || linea.Contains('\n'))
                    throw new CertificadosRetencionDdjjException("Ganancias devolvió una línea nula, vacía o con saltos internos. Solicite revisar el SP y las órdenes de pago. No se entregó un TXT parcial.");
                // Sin encabezado, Trim, separadores ni reconstrucción del registro fiscal.
                await writer.WriteLineAsync(linea.AsMemory(), ct); cantidad++;
            }
            if (cantidad == 0) throw new CertificadosRetencionDdjjException("No hay registros para la DDJJ mensual seleccionada. No se generó un TXT vacío.");
            return;
        }
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("DDJJ " + impuesto); hoja.ShowGridLines = false;
        hoja.Style.Font.FontName = "Arial"; hoja.Style.Font.FontSize = 11;
        for (var c = 0; c < columnas.Length; c++)
        {
            hoja.Cell(1, c + 1).Value = columnas[c].Titulo;
            hoja.Column(c + 1).Width = columnas[c].Ancho;
            hoja.Column(c + 1).Style.NumberFormat.Format = columnas[c].Tipo == typeof(string) ? "@" : columnas[c].Tipo == typeof(DateTime) ? "dd/mm/yyyy" : "#,##0.00;[Red]-#,##0.00";
        }
        var encabezado = hoja.Range(1, 1, 1, columnas.Length);
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#846313"); encabezado.Style.Font.FontColor = XLColor.White;
        encabezado.Style.Font.Bold = true; encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        hoja.Row(1).Height = 26; hoja.SheetView.FreezeRows(1);
        var r = 1;
        await foreach (var fila in filas.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            ValidarFila(fila, columnas);
            if (r == 1048576) throw new CertificadosRetencionDdjjException("La DDJJ supera la capacidad de una hoja Excel. No se entregó una exportación truncada.");
            r++;
            for (var c = 0; c < fila.Length; c++)
            {
                var celda = hoja.Cell(r, c + 1);
                switch (fila[c])
                {
                    // Value de texto, nunca FormulaA1: preservar ceros y evitar fórmulas del origen.
                    case string texto: celda.Value = texto; break;
                    case decimal importe: celda.Value = importe; break;
                    case DateTime fecha: celda.Value = fecha; break;
                }
            }
            if (r % 2 == 0) hoja.Range(r, 1, r, columnas.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FAF6EB");
        }
        if (r == 1) throw new CertificadosRetencionDdjjException("No hay registros para la DDJJ mensual seleccionada. No se generó un Excel vacío.");
        hoja.Range(1, 1, r, columnas.Length).SetAutoFilter();
        ct.ThrowIfCancellationRequested(); libro.SaveAs(destino); ct.ThrowIfCancellationRequested();
    }
    private static void ValidarFila(object?[] fila, Columna[] columnas)
    {
        if (fila.Length != columnas.Length || fila.Where((valor, c) => valor != null && !columnas[c].Tipo.IsInstanceOfType(valor)).Any())
            throw new InvalidOperationException("El contrato de columnas de DDJJ cambió. No se entregó un archivo parcial.");
    }
}
public class CertificadosRetencionDdjjException(string mensaje) : InvalidOperationException(mensaje) { }
