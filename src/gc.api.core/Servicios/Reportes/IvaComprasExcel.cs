using ClosedXML.Excel;
using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Servicios.Reportes;

public static class IvaComprasExcel
{
    public const string Mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public static byte[] Generar(IReadOnlyList<IvaComprasDetalleDto> filas, IvaComprasFiltro filtro, CancellationToken ct = default)
    {
        using var stream = new MemoryStream();
        Escribir(filas, filtro, stream, ct);
        return stream.ToArray();
    }

    // API Report escribe a archivo temporal: no duplica el XLSX completo en byte[].
    public static void Escribir(IReadOnlyList<IvaComprasDetalleDto> filas, IvaComprasFiltro filtro, Stream destino, CancellationToken ct = default)
    {
        Libros.IvaComprasServicio.ValidarExportacionCompleta(filas);
        using var libro = new XLWorkbook();
        var columnas = IvaComprasColumnas.Todas;
        var propiedades = columnas.Select(c => typeof(IvaComprasDetalleDto).GetProperty(c.Campo)!).ToArray();
        const int filasPorHoja = 1_000_000; // No truncar al alcanzar el límite de una hoja XLSX.
        for (int inicio = 0, numero = 1; inicio < filas.Count; inicio += filasPorHoja, numero++)
        {
            var hoja = libro.AddWorksheet($"IVA Compras {numero}");
            hoja.Cell(1, 1).Value = "Libro IVA Compras - Detalle";
            hoja.Range(1, 1, 1, columnas.Length).Merge();
            hoja.Cell(2, 1).Value = filtro.Descripcion;
            hoja.Range(2, 1, 2, columnas.Length).Merge();
            hoja.Cell(3, 1).Value = $"Comprobantes exportados: {filas.Count:N0}. Consulta nueva al exportar; puede diferir si los datos cambiaron.";
            hoja.Range(3, 1, 3, columnas.Length).Merge();
            hoja.Cell(4, 1).Value = "Los importes de más de 15 dígitos significativos se conservan como texto exacto para evitar redondeos de Excel.";
            hoja.Range(4, 1, 4, columnas.Length).Merge();
            for (int c = 0; c < columnas.Length; c++) hoja.Cell(5, c + 1).Value = columnas[c].Titulo;
            int cantidad = Math.Min(filasPorHoja, filas.Count - inicio);
            for (int i = 0; i < cantidad; i++)
            {
                ct.ThrowIfCancellationRequested();
                for (int c = 0; c < propiedades.Length; c++)
                {
                    var valor = propiedades[c].GetValue(filas[inicio + i]);
                    var celda = hoja.Cell(i + 6, c + 1);
                    // Value (no FormulaA1) conserva identificadores, ceros y textos que comienzan con '='.
                    if (valor is string texto) celda.Value = texto;
                    else if (valor is decimal importe)
                    {
                        if (RequiereTextoExacto(importe)) celda.Value = importe.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else celda.Value = importe;
                    }
                    else if (valor is DateTime fecha) celda.Value = fecha;
                }
            }
            for (int c = 0; c < propiedades.Length; c++)
            {
                var tipo = Nullable.GetUnderlyingType(propiedades[c].PropertyType) ?? propiedades[c].PropertyType;
                hoja.Column(c + 1).Width = columnas[c].Campo == "nombre" ? 42 : 20;
                if (tipo == typeof(decimal)) hoja.Range(6, c + 1, cantidad + 5, c + 1).Style.NumberFormat.Format = "#,##0.00;[Red]-#,##0.00";
                if (tipo == typeof(DateTime)) hoja.Range(6, c + 1, cantidad + 5, c + 1).Style.DateFormat.Format = "dd/MM/yyyy";
                if (tipo == typeof(string)) hoja.Range(6, c + 1, cantidad + 5, c + 1).Style.NumberFormat.Format = "@";
            }
            hoja.Range(5, 1, 5, columnas.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#C79816");
            hoja.Range(5, 1, 5, columnas.Length).Style.Font.Bold = true;
            hoja.Range(5, 1, cantidad + 5, columnas.Length).SetAutoFilter();
            hoja.SheetView.FreezeRows(5);
        }
        ct.ThrowIfCancellationRequested();
        libro.SaveAs(destino);
        ct.ThrowIfCancellationRequested();
    }

    public static bool RequiereTextoExacto(decimal importe)
    {
        var mantisa = importe.ToString("G29", System.Globalization.CultureInfo.InvariantCulture).Split('E', 'e')[0];
        return new string(mantisa.Where(char.IsDigit).ToArray()).TrimStart('0').Length > 15;
    }
}
