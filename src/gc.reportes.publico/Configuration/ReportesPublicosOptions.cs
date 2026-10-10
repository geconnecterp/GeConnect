using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Geco.Reportes.Publico.Configuration;

public sealed class ReportesPublicosOptions
{
    public const string Section = "ReportesPublicos";
    [Required] public string ApiBaseUrl { get; set; } = "";
    [Range(5, 300)] public int TimeoutSegundos { get; set; } = 90;
    [Range(1, 50)] public int MaxPdfMb { get; set; } = 20;
    public bool RequiereControlDescargas { get; set; } = true;
    [Range(1, 16)] public int MaxConcurrentes { get; set; } = 4;
    [Range(1, 300)] public int SolicitudesPorMinutoPorIp { get; set; } = 20;
    [Range(1, 1000)] public int SolicitudesPorMinutoGlobal { get; set; } = 120;
    public string[] ProxiesConfiables { get; set; } = [];

    public static bool EsUrlHttps(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
        && !string.IsNullOrEmpty(uri.Host) && string.IsNullOrEmpty(uri.UserInfo)
        && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment);

    public bool ProxiesValidos() => ProxiesConfiables.All(p => IPAddress.TryParse(p, out _));
}
