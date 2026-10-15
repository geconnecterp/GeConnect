using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

// Opt-in: consulta acotada al período reportado, sin escribir datos ni exportar el mes entero.
internal static class IvaVentasSqlCheck
{
    public static async Task Run()
    {
        var config = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.json"));
        var connection = config["ConnectionStrings"]?["GeConnectKey"]?.Value<string>();
        if (File.Exists("gc.api/appsettings.Development.json"))
        {
            var dev = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.Development.json"));
            connection = dev["ConnectionStrings"]?["GeConnectKey"]?.Value<string>() ?? connection;
        }
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Falta la conexión de desarrollo.");
        await using var context = new GeConnectContext(new DbContextOptionsBuilder<GeConnectContext>().UseSqlServer(connection).Options);
        var service = new IvaVentasServicio(new IvaVentasDatos(context), null!);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var filter = new IvaVentasFiltro { Periodo = 2025, Mes = 11, Registros = 100, Pagina = 1 };
        var first = await service.Consultar(filter, timeout.Token);
        if (first.Total <= 100) throw new InvalidOperationException("El período no permite comprobar varias páginas.");
        Verify(first, 1, 100, first.Total);
        filter.Pagina = 2;
        Verify(await service.Consultar(filter, timeout.Token), 2, 100, first.Total);
        filter.Pagina = first.Paginas;
        Verify(await service.Consultar(filter, timeout.Token), first.Paginas, 100, first.Total);
        filter.Pagina = 1; filter.Registros = 500;
        Verify(await service.Consultar(filter, timeout.Token), 1, 500, first.Total);
        Console.WriteLine("PASS SQL: lectura y paginación C# de noviembre 2025, sin cambios en SP ni datos.");
    }

    private static void Verify(IvaVentasPagina page, int number, int size, int total)
    {
        var expected = Math.Min(size, Math.Max(0, total - (number - 1) * size));
        if (page.Pagina != number || page.Registros != size || page.Total != total || page.Filas.Count != expected)
            throw new InvalidOperationException("Paginación real inconsistente o datos cambiados durante la prueba.");
        Console.WriteLine($"OK SQL: página {page.Pagina}/{page.Paginas}, {page.Filas.Count} filas, total {page.Total}; CAE nulos {page.Filas.Count(x => !x.cm_cae_vto.HasValue)}.");
    }
}
