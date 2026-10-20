using gc.infraestructura.Dtos.Libros;
using System.IO.Compression;
using System.Text;

namespace gc.api.core.Servicios.Libros;

public record IvaComprasCierreConteos(long Cabecera, long Detalle, long LongitudesCabecera, long LongitudesDetalle);

public static class IvaComprasCierreArchivos
{
    // Convención provisional de Ventas; validar encoding/layout con el tester, no certificación fiscal.
    public static string NombreZip(string periodo) => $"IVA_Compras_3685_{IvaComprasCierrePeriodo.Texto(periodo)}.zip";

    public static async Task<IvaComprasCierreConteos> EscribirZip(Stream destino, string periodo,
        Func<char, IAsyncEnumerable<string?>> lineas, CancellationToken ct)
    {
        IvaComprasCierrePeriodo.Validar(periodo);
        long cabecera = 0, detalle = 0, largoC = 0, largoD = 0;
        ct.ThrowIfCancellationRequested();
        using var zip = new ZipArchive(destino, ZipArchiveMode.Create, leaveOpen: true);
        foreach (var (tipo, nombre, nominal) in new[] { ('C', "Cabecera", 325), ('D', "Detalle", 84) })
        {
            ct.ThrowIfCancellationRequested();
            var entry = zip.CreateEntry($"IVA_Compras_3685_{IvaComprasCierrePeriodo.Texto(periodo)}_{nombre}.txt", CompressionLevel.Fastest);
            await using var stream = entry.Open();
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false, true), 65536, leaveOpen: true) { NewLine = "\r\n" };
            await foreach (var linea in lineas(tipo).WithCancellation(ct))
            {
                ct.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(linea))
                    throw new IvaComprasCierreArchivoException($"RG 3685 devolvió un registro nulo o vacío en {tipo}. Solicite revisar los datos del período. No se entregó un archivo parcial.");
                if (linea.Contains('\r') || linea.Contains('\n'))
                    throw new IvaComprasCierreArchivoException($"RG 3685 devolvió saltos dentro de un registro de {tipo}. No se entregó un archivo parcial.");
                // Preservar texto, espacios y orden; los anchos nominales sólo se diagnostican.
                if (tipo == 'C') { checked { cabecera++; if (linea.Length != nominal) largoC++; } }
                else { checked { detalle++; if (linea.Length != nominal) largoD++; } }
                await writer.WriteLineAsync(linea.AsMemory(), ct);
            }
        }
        // Cero filas deja una entrada vacía; no fabricar registros para una rama D sin alícuotas.
        return new(cabecera, detalle, largoC, largoD);
    }
}

