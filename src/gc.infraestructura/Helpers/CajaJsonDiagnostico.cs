using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.infraestructura.Core.Helpers;

// Representación legible exclusivamente para logs; no modifica el contrato HTTP ni los datos.
public static class CajaJsonDiagnostico
{
    public static string Formatear(string texto, string token = "", bool expandirJsonAnidado = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            var contenido = JsonConvert.DeserializeObject<JToken>(texto, new JsonSerializerSettings {
                DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal,
                MaxDepth = 64, CheckAdditionalContent = true
            });
            var documento = contenido ?? JValue.CreateNull();
            texto = (expandirJsonAnidado ? ExpandirJsonParaLog(documento, 0) : documento).ToString(Formatting.Indented);
        }
        catch (JsonException) { /* Conservar también cuerpos de error que no sean JSON. */ }
        return string.IsNullOrEmpty(token) ? texto : texto.Replace(token, "[TOKEN OMITIDO]", StringComparison.Ordinal);
    }

    // Sólo para la vista del log: no cambia el cuerpo HTTP ni los DTO recibidos.
    // Por ejemplo, muestra resultado_id con JSON anidado como arreglo, sin comillas duplicadas.
    private static JToken ExpandirJsonParaLog(JToken contenido, int profundidad)
    {
        if (profundidad >= 16) return contenido;
        if (contenido is JObject objeto)
            foreach (var propiedad in objeto.Properties()) propiedad.Value = ExpandirJsonParaLog(propiedad.Value, profundidad + 1);
        else if (contenido is JArray lista)
            for (var i = 0; i < lista.Count; i++) lista[i] = ExpandirJsonParaLog(lista[i], profundidad + 1);
        else if (contenido.Type == JTokenType.String)
        {
            var texto = contenido.Value<string>()?.Trim();
            if (!string.IsNullOrEmpty(texto) && (texto.StartsWith('{') || texto.StartsWith('[')))
                try
                {
                    var anidado = JsonConvert.DeserializeObject<JToken>(texto, new JsonSerializerSettings {
                        DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal,
                        MaxDepth = 64, CheckAdditionalContent = true
                    });
                    if (anidado is JObject or JArray) return ExpandirJsonParaLog(anidado, profundidad + 1);
                }
                catch (JsonException) { }
        }
        return contenido;
    }
}
