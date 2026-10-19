using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

// Opt-in explícito, solo lectura y acotado: no se exporta el período entero ni se escribe en SQL.
internal static class IvaComprasSqlCheck
{
    public static async Task Run()
    {
        try
        {
            var config = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.json"));
            var connection = config["ConnectionStrings"]?["GeConnectKey"]?.Value<string>();
            if (File.Exists("gc.api/appsettings.Development.json"))
            {
                var dev = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.Development.json"));
                connection = dev["ConnectionStrings"]?["GeConnectKey"]?.Value<string>() ?? connection;
            }
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Falta configuración.");
            await using var context = new GeConnectContext(new DbContextOptionsBuilder<GeConnectContext>().UseSqlServer(connection).Options);
            var datos = new IvaComprasDatos(context);
            var service = new IvaComprasServicio(datos, null!);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var options = await datos.Opciones(timeout.Token);
            Console.WriteLine($"OK SQL: {options.TiposProveedor.Count} tipos proveedor y {options.Comprobantes.Count} comprobantes; {watch.ElapsedMilliseconds} ms.");
            var filtro = new IvaComprasFiltro { Periodo = 2025, Mes = 10, Registros = 100, Pagina = 1 };
            var first = await service.Consultar(filtro, timeout.Token);
            Console.WriteLine($"OK SQL: página {first.Pagina}/{first.Paginas}; filas {first.Filas.Count}; total {first.Total}; {watch.ElapsedMilliseconds} ms acumulados.");
            if (first.Paginas > 1)
            {
                filtro.Pagina = first.Paginas;
                var last = await service.Consultar(filtro, timeout.Token);
                if (last.Total != first.Total || last.Pagina != first.Paginas ||
                    first.Filas.Select(Key).Intersect(last.Filas.Select(Key)).Any())
                    throw new InvalidOperationException("Cambios concurrentes o paginación inconsistente.");
                Console.WriteLine($"OK SQL: última {last.Pagina}/{last.Paginas}; filas {last.Filas.Count}; total {last.Total}; {watch.ElapsedMilliseconds} ms acumulados.");
            }
            Console.WriteLine("PASS SQL: materialización y extremos del período 2025-10. Sin exportar ni modificar datos.");
        }
        catch (Exception ex)
        {
            // No imprimir valores, host, credenciales ni mensajes SQL con datos fiscales.
            Console.WriteLine($"FAIL SQL: {ex.GetType().Name}; campo {(ex as IvaComprasDatoInvalidoException)?.Campo ?? "no aplica"}; código {(ex as Microsoft.Data.SqlClient.SqlException)?.Number}.");
            Environment.ExitCode = 1;
        }
    }

    private static (string, string, string, string) Key(IvaComprasDetalleDto f) => (f.tipo, f.compte, f.movi, f.cta_id);
}
