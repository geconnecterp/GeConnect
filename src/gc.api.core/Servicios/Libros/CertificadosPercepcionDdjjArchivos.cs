using ClosedXML.Excel;
using System.Data.Common;

namespace gc.api.core.Servicios.Libros;

public static class CertificadosPercepcionDdjjArchivos
{
    public record Columna(string Campo, string Titulo, Type Tipo, double Ancho);
    public static Columna[] Columnas(string impuesto)
    {
        if (impuesto is not ("IB" or "IV")) throw new ArgumentException("Impuesto de DDJJ no soportado.");
        var columnas = new List<Columna>
        {
            new("razon_social", "Razón social", typeof(string), 44), new("cuit", "CUIT", typeof(string), 18),
            new("domicilio", "Domicilio", typeof(string), 62)
        };
        // El SP IB devuelve cm_cuit como ib_nro. Conservarlo, no reconstruir una inscripción.
        if (impuesto == "IB") columnas.Add(new("ib_nro", "CUIT (2C)", typeof(string), 18));
        columnas.AddRange([
            new("fecha", "Fecha", typeof(DateTime), 14), new("tco", "Tipo", typeof(string), 12),
            new("compte", "Nro. comprobante", typeof(string), 24), new("base", "Base imponible", typeof(decimal), 25),
            new("ali", "Alícuota (%)", typeof(decimal), 16), new("percepcion", "Percepción", typeof(decimal), 25)
        ]);
        if (impuesto == "IB") columnas.Add(new("percepcion_lh", "Percepción LH", typeof(decimal), 25));
        columnas.AddRange([new("tco_id_ori", "Tipo original", typeof(string), 16), new("cm_compte_ori", "Nro. original", typeof(string), 24)]);
        return columnas.ToArray();
    }
    public static void ValidarColumnas(DbDataReader reader, string impuesto)
    {
        var columnas = Columnas(impuesto);
        if (reader.FieldCount != columnas.Length) throw new InvalidOperationException("La DDJJ AP devolvió una cantidad de columnas distinta al contrato.");
        for (var c = 0; c < columnas.Length; c++)
            if (reader.GetFieldType(c) != columnas[c].Tipo || !string.Equals(reader.GetName(c), columnas[c].Campo, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("La DDJJ AP devolvió columnas o tipos diferentes al contrato.");
    }
    public static async Task Escribir(Stream destino, string impuesto, IAsyncEnumerable<object?[]> filas, CancellationToken ct)
    {
        var columnas = Columnas(impuesto);
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("DDJJ " + impuesto); hoja.ShowGridLines = false;
        hoja.Style.Font.FontName = "Arial"; hoja.Style.Font.FontSize = 11;
        for (var c = 0; c < columnas.Length; c++)
        {
            hoja.Cell(1, c + 1).Value = columnas[c].Titulo; hoja.Column(c + 1).Width = columnas[c].Ancho;
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
            if (fila.Length != columnas.Length || fila.Where((v, c) => v != null && !columnas[c].Tipo.IsInstanceOfType(v)).Any())
                throw new InvalidOperationException("El contrato de DDJJ AP cambió. No se entregó un archivo parcial.");
            if (r == 1048576) throw new CertificadosPercepcionDdjjException("La DDJJ supera la capacidad de una hoja Excel. No se entregó una exportación truncada.");
            r++;
            for (var c = 0; c < fila.Length; c++)
            {
                var celda = hoja.Cell(r, c + 1);
                switch (fila[c])
                {
                    // Texto explícito: ceros iniciales intactos y sin fórmulas provenientes del origen.
                    case string texto: celda.Value = texto; break;
                    case decimal importe: celda.Value = importe; break;
                    case DateTime fecha: celda.Value = fecha; break;
                    // NULL queda vacío, nunca cero.
                }
            }
            if (r % 2 == 0) hoja.Range(r, 1, r, columnas.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FAF6EB");
        }
        if (r == 1) throw new CertificadosPercepcionDdjjException("No hay registros para la DDJJ mensual seleccionada. No se generó un Excel vacío.");
        hoja.Range(1, 1, r, columnas.Length).SetAutoFilter();
        ct.ThrowIfCancellationRequested(); libro.SaveAs(destino); ct.ThrowIfCancellationRequested();
    }
}
public class CertificadosPercepcionDdjjException(string mensaje) : InvalidOperationException(mensaje) { }
