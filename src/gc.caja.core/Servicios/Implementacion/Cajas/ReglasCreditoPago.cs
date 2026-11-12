using gc.infraestructura.Dtos.Cajas;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class ReglasCreditoPago
{
    public sealed record Condiciones(bool registrado, decimal tope, int? dias_cheque, int? dias_documento);
    public sealed record Resultado(bool Ok, string Mensaje, bool ExcedenteCheques = false);

    public static Condiciones ObtenerCondiciones(CuentaDatosResultadoDto cliente, Action<string>? diagnostico = null)
    {
        JArray? formas = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(cliente.fp))
            {
                var raiz = JToken.Parse(cliente.fp);
                formas = raiz as JArray ?? (raiz as JObject)?.GetValue("fp", StringComparison.OrdinalIgnoreCase) as JArray;
                if (formas == null) diagnostico?.Invoke("fp debe contener un arreglo o un objeto con el arreglo fp.");
            }
        }
        catch (JsonException) { diagnostico?.Invoke("fp no contiene JSON válido."); }

        int? Dias(string tipo)
        {
            if (formas == null) return null;
            var opciones = formas.OfType<JObject>().Where(f =>
                f["fp_id"]?.Type == JTokenType.String &&
                string.Equals(f["fp_id"]!.Value<string>()?.Trim(), tipo, StringComparison.OrdinalIgnoreCase)).ToList();
            if (opciones.Count == 0) return null;
            int? minimo = null;
            foreach (var opcion in opciones)
            {
                var valor = opcion["fp_dias"];
                if (valor?.Type is not (JTokenType.Integer or JTokenType.Float or JTokenType.String) ||
                    !decimal.TryParse(valor.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dias) ||
                    dias < 1 || dias > int.MaxValue || decimal.Truncate(dias) != dias)
                {
                    diagnostico?.Invoke($"fp_id={tipo}: fp_dias debe ser un entero positivo; medio no habilitado.");
                    return null;
                }
                minimo = Math.Min(minimo ?? int.MaxValue, (int)dias);
            }
            return minimo;
        }
        return new(string.Equals(cliente.Origen?.Trim(), "C", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(cliente.cta_id), cliente.ctac_tope_credito, Dias("H"), Dias("D"));
    }

    public static Resultado Validar(IReadOnlyCollection<Json_Valor> valores, decimal total, decimal nc,
        string coTipo, Condiciones condiciones, IReadOnlyDictionary<string, string> instrumentos, DateTime hoy)
    {
        string Tipo(Json_Valor v) => DocumentoCuentaCorriente.EsDocumento(v) ? "DO" :
            instrumentos.TryGetValue(v.ins_id?.Trim() ?? "", out var tipo) ? tipo.Trim().ToUpperInvariant() : "";
        if (valores.Any(v => Tipo(v) == ""))
            return new(false, "No se pudo identificar un instrumento del pago. Recargue los medios de pago antes de confirmar.");
        var credito = valores.Where(v => Tipo(v) is "CH" or "DO").ToList();
        if (credito.Count > 0)
        {
            if (!condiciones.registrado) return new(false, "Cheques y documentos requieren un cliente registrado con cta_id válido.");
            if (condiciones.tope <= 0 || credito.Sum(v => v.rb_importe) > condiciones.tope)
                return new(false, "La suma de cheques y documentos supera el tope de crédito del cliente.");
            foreach (var v in credito)
            {
                var dias = Tipo(v) == "CH" ? condiciones.dias_cheque : condiciones.dias_documento;
                if (dias is null || dias < 1) return new(false, "El cliente no tiene un plazo válido configurado para esta forma de pago.");
                var fecha = v.rb_fecha_valor.Date;
                if (fecha < hoy.Date || (fecha - hoy.Date).TotalDays > dias)
                    return new(false, $"El vencimiento debe estar entre hoy y {dias} días desde hoy, según la forma de pago del cliente.");
            }
        }
        var excedente = valores.Sum(v => v.rb_importe) + nc - total;
        var soloCheques = (coTipo is "CD" or "CC") && nc == 0 && valores.Count > 0 && valores.All(v => Tipo(v) == "CH");
        if (excedente > 0 && !soloCheques)
        {
            var efectivo = valores.Where(v => Tipo(v) == "EF").Sum(v => v.rb_importe);
            if (efectivo < excedente)
                return new(false, "El pago supera el total permitido. Sólo se admite vuelto de efectivo o una cobranza pagada exclusivamente con cheques dentro del tope de crédito.");
        }
        return new(true, "", excedente > 0 && soloCheques);
    }
}
