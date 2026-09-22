using gc.infraestructura.Dtos.Cajas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.caja.Models;

public static class SorteosFactura
{
    public static List<FactSorteoJsonDto> DesdeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        var token = JToken.Parse(json);
        if (token.Type == JTokenType.Null || token is JObject { Count: 0 }) return [];
        if (token is not JArray registros)
            throw new JsonSerializationException("El JSON de sorteos debe ser una lista.");

        var sorteos = new List<FactSorteoJsonDto>();
        foreach (var registro in registros)
        {
            if (registro.Type == JTokenType.Null || registro is JObject { Count: 0 }) continue;
            if (registro is not JObject)
                throw new JsonSerializationException("Registro de sorteo inválido.");
            var sorteo = registro.ToObject<FactSorteoJsonDto>();
            if (sorteo == null || string.IsNullOrWhiteSpace(sorteo.So_Sorteo))
                throw new JsonSerializationException("El sorteo no tiene identificador.");
            sorteos.Add(sorteo);
        }
        return sorteos;
    }
}
