using gc.api.infra.Datos;
using gc.api.infra.Datos.Implementacion;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;

// Opt-in y sólo lectura: nunca llama al SP de cierre ni genera archivos fiscales.
internal static class IvaVentasCierreSqlLectura
{
    public static async Task Run()
    {
        var json = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.json"));
        var connection = json["ConnectionStrings"]?["GeConnectKey"]?.Value<string>();
        if (File.Exists("gc.api/appsettings.Development.json"))
        {
            var dev = JObject.Parse(await File.ReadAllTextAsync("gc.api/appsettings.Development.json"));
            connection = dev["ConnectionStrings"]?["GeConnectKey"]?.Value<string>() ?? connection;
        }
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Falta la conexión de desarrollo.");
        connection = connection.Replace(@"\\", @"\");
        await using var context = new GeConnectContext(new DbContextOptionsBuilder<GeConnectContext>().UseSqlServer(connection).Options);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var datos = new IvaVentasCierreDatos(context);
        var estado = await datos.Consultar(timeout.Token);
        estado.Validar();
        if (!estado.Libros.Select(x => x.periodo).SequenceEqual(estado.Libros.Select(x => x.periodo).OrderByDescending(x => x, StringComparer.Ordinal)))
            throw new InvalidOperationException("El historial real no está ordenado de forma descendente.");
        if (estado.Libros.Count > 0)
        {
            var ultimo = estado.Libros[0].periodo;
            var siguiente = DateTime.ParseExact(ultimo, "yyyyMM", CultureInfo.InvariantCulture).AddMonths(1).ToString("yyyyMM", CultureInfo.InvariantCulture);
            if (estado.ProximoPeriodo != siguiente || !await datos.ExisteCierre(ultimo, timeout.Token))
                throw new InvalidOperationException("La función o la consulta de existencia no coincide con el historial real.");
        }
        Console.WriteLine($"PASS SQL sólo lectura: {estado.Libros.Count} filas mapeadas, orden descendente, función y existencia del último cierre. No se invocó SPGECO_IVA_V_Cerrar.");
    }
}
