using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

try
{
    string? Opcion(string clave) { var i = Array.IndexOf(args, clave); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
    var ruta = Path.GetFullPath(Opcion("--config") ?? Path.Combine(AppContext.BaseDirectory, "cajasettings.json"));
    if (!File.Exists(ruta)) throw new InvalidOperationException($"No se encontró la configuración del puesto: {ruta}");
    var json = await File.ReadAllTextAsync(ruta);
    Configuracion config;
    try
    {
        config = JsonSerializer.Deserialize<Configuracion>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException($"El archivo de configuración está vacío: {ruta}");
    }
    catch (JsonException)
    {
        throw new InvalidOperationException($"El archivo de configuración no contiene JSON válido: {ruta}. Revise su contenido.");
    }
    config.EstacionId = string.IsNullOrWhiteSpace(config.EstacionId) ? Environment.MachineName : config.EstacionId;
    Validator.ValidateObject(config, new ValidationContext(config), true);
    var servidor = Opcion("--servidor") ?? config.Servidor;
    if (!Uri.TryCreate(servidor?.TrimEnd('/') + "/", UriKind.Absolute, out var destino) || destino.Scheme != "https" || !string.IsNullOrEmpty(destino.UserInfo) || !string.IsNullOrEmpty(destino.Query) || !string.IsNullOrEmpty(destino.Fragment))
        throw new InvalidOperationException("Configure Servidor o --servidor con la URL HTTPS de GECO Caja.");
    if (args.Contains("--validar")) { Console.WriteLine($"Configuración válida. Puesto {config.EstacionId}, caja {config.CajaId}, sucursal {config.AdmId}."); return 0; }
    // Una redirección a login no es una preparación válida: no seguirla ni reenviar la configuración.
    using var handler = new HttpClientHandler { AllowAutoRedirect = false };
    using var cliente = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
    cliente.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    var endpoint = new Uri(destino, "Seguridad/Estacion/Preparar");
    using var respuesta = await cliente.PostAsJsonAsync(endpoint, new {
        config.EstacionId, config.CajaId, config.AdmId, config.IP, config.Facturacion, config.TipoCnnCF, config.Acumula
    });
    var pase = await RespuestaInicio.LeerPaseAsync(respuesta, endpoint);
    var url = new Uri(destino, "Seguridad/Estacion/Iniciar?pase=" + pase).AbsoluteUri;
    if (args.Contains("--sin-abrir")) Console.WriteLine(url); // Diagnóstico: pase efímero, de un solo uso.
    else Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("No se pudo abrir GECO Caja. " + ex.Message);
    if (!args.Contains("--sin-abrir") && !args.Contains("--validar") && !Console.IsInputRedirected) { Console.WriteLine("Presione Enter para cerrar."); Console.ReadLine(); }
    return 1;
}

sealed class Configuracion
{
    public string? Servidor { get; set; }
    [Required, StringLength(80), RegularExpression(@"[A-Za-z0-9_.-]+")]
    public string EstacionId { get; set; } = "";
    [Required, StringLength(4), RegularExpression(@"[A-Za-z0-9]+")]
    public string CajaId { get; set; } = "";
    [Required, StringLength(10), RegularExpression(@"[A-Za-z0-9]+")]
    public string AdmId { get; set; } = "";
    [Required, StringLength(200), RegularExpression(@"[^\r\n]+")]
    public string IP { get; set; } = "";
    [Range(1, 2)] public int Facturacion { get; set; }
    [Range(0, 2)] public int TipoCnnCF { get; set; }
    public bool Acumula { get; set; }
}
