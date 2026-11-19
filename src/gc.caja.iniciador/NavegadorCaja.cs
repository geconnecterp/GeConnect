using System.Diagnostics;
using Microsoft.Win32;

internal static class NavegadorCaja
{
    public static ProcessStartInfo Preparar(string modo, string? rutaChrome)
    {
        if (modo == "PREDETERMINADO") return new ProcessStartInfo { UseShellExecute = true };

        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("La ventana de Chrome del iniciador requiere Windows.");

        if (!string.IsNullOrWhiteSpace(rutaChrome))
        {
            var ruta = Path.GetFullPath(Environment.ExpandEnvironmentVariables(rutaChrome.Trim().Trim('"')));
            if (!File.Exists(ruta))
                throw new InvalidOperationException($"No se encontró Chrome en ChromeRuta: {ruta}");
            return new ProcessStartInfo(ruta) { UseShellExecute = false };
        }

        const string registroChrome = @"Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
        var rutas = new[]
        {
            Registry.GetValue(@"HKEY_CURRENT_USER\" + registroChrome, "", null) as string,
            Registry.GetValue(@"HKEY_LOCAL_MACHINE\" + registroChrome, "", null) as string,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Google\Chrome\Application\chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
        };
        var instalada = rutas.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r) && File.Exists(r.Trim('"')));
        if (instalada is null)
            throw new InvalidOperationException("No se encontró Google Chrome. Instálelo en este puesto o indique su ejecutable en ChromeRuta del JSON local. Para abrir con el navegador predeterminado, configure NavegadorModo como PREDETERMINADO.");

        return new ProcessStartInfo(instalada.Trim('"')) { UseShellExecute = false };
    }
}
