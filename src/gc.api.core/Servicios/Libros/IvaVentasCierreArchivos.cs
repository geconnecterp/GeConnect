using gc.infraestructura.Dtos.Libros;
using System.IO.Compression;
using System.Text;

namespace gc.api.core.Servicios.Libros;

public static class IvaVentasCierreArchivos
{
    // Convención provisional solicitada para revisión del tester, no certificación de formato fiscal.
    public static string NombreZip(string periodo) => $"IVA_Ventas_3685_{IvaVentasCierrePeriodo.Texto(periodo)}.zip";
    public static async Task EscribirZip(Stream destino, string periodo, Func<char, IAsyncEnumerable<string?>> lineas, CancellationToken ct)
    {
        IvaVentasCierrePeriodo.Validar(periodo);
        using var zip = new ZipArchive(destino, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var (tipo, nombre) in new[] { ('C', "Cabecera"), ('D', "Detalle") })
        {
            ct.ThrowIfCancellationRequested();
            var entry = zip.CreateEntry($"IVA_Ventas_3685_{IvaVentasCierrePeriodo.Texto(periodo)}_{nombre}.txt", CompressionLevel.Fastest);
            await using var stream = entry.Open();
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false, true), 65536, leaveOpen: true) { NewLine = "\r\n" };
            await foreach (var linea in lineas(tipo).WithCancellation(ct))
            {
                if (linea == null) throw new InvalidOperationException($"El SP RG 3685 devolvió una línea nula en {tipo}. No se generó una descarga parcial.");
                if (linea.Contains('\r') || linea.Contains('\n'))
                    throw new InvalidOperationException($"El SP RG 3685 devolvió saltos de línea dentro de un registro de {tipo}.");
                // No Trim, recálculos, encabezados, separadores, ni conversiones numéricas.
                await writer.WriteLineAsync(linea.AsMemory(), ct);
            }
        }
    }
}
