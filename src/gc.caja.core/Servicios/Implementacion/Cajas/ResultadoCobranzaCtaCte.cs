using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class ResultadoCobranzaCtaCte
{
    // Ope_Confirmar puede devolver el ID simple o el JSON de recibo.
    // cm_compte no se usa como sustituto: identifica otro tipo de comprobante.
    public static string? ObtenerNumeroRecibo(string? resultadoId)
    {
        var texto = resultadoId?.Trim();
        if (string.IsNullOrEmpty(texto)) return null;
        if (texto.StartsWith("[") || texto.StartsWith("{") || texto.StartsWith("\""))
        {
            try
            {
                var dato = JToken.Parse(texto);
                if (dato is JArray lista)
                {
                    if (lista.Count != 1) return null;
                    dato = lista[0];
                }
                if (dato is JObject objeto) dato = objeto["rb_compte"];
                if (dato == null || (dato.Type != JTokenType.String && dato.Type != JTokenType.Integer)) return null;
                texto = dato.ToString().Trim();
            }
            catch (JsonException) { return null; }
        }
        return Regex.IsMatch(texto, @"^(?=.*[0-9])[\p{L}\p{N}][\p{L}\p{N}._/-]*$") ? texto : null;
    }
}
