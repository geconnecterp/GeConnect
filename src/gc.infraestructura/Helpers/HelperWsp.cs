using System.Diagnostics;
using System.Text.RegularExpressions;

namespace gc.infraestructura.Helpers
{
    public static class HelperWsp
    {
        public static void EnviarMensaje(string numero, string mensaje)
        {
            try
            {

                // Limpiar el número de teléfono
                numero = Regex.Replace(numero, @"[\s\-\.\(\)]", "");

                string url = $"https://wa.me/{numero}?text={Uri.EscapeDataString(mensaje)}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }   
    }
}
