using gc.infraestructura.Dtos.Libros;
using System.IO.Compression;
using System.Text;

namespace gc.api.core.Servicios.Libros;

public static class IvaVentasCierreArchivos
{
    // Convención provisional solicitada para revisión del tester, no certificación de formato fiscal.
    public static string NombreZip(string periodo) => $"IVA_Ventas_3685_{IvaVentasCierrePeriodo.Texto(periodo)}.zip";
    public static string NombreDuplicados(string periodo) => $"IVA_Ventas_Duplicados_{IvaVentasCierrePeriodo.Texto(periodo)}.txt";
    public static async Task EscribirDuplicados(Stream destino, IAsyncEnumerable<string?> lineas, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await using var writer = new StreamWriter(destino, new UTF8Encoding(false, true), 65536, leaveOpen: true) { NewLine = "\r\n" };
        var hayRegistros = false;
        await foreach (var linea in lineas.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(linea))
                throw new IvaVentasCierreArchivoException("Duplicados Electrónicos devolvió un registro nulo o vacío. Solicite al DBA revisar el período y las sumatorias del SP. No se entregó un archivo parcial.");
            if (linea.Contains('\r') || linea.Contains('\n'))
                throw new IvaVentasCierreArchivoException("Duplicados Electrónicos devolvió un salto de línea dentro de un registro. Solicite revisar el origen de datos. No se entregó un archivo parcial.");
            // La aplicación no reconstruye los registros ni modifica el orden recibido del SP.
            await writer.WriteLineAsync(linea.AsMemory(), ct);
            hayRegistros = true;
        }
        if (!hayRegistros)
            throw new IvaVentasCierreArchivoException("Duplicados Electrónicos no devolvió registros. No se generó un TXT vacío; solicite revisar el período en el SP.");
    }
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
