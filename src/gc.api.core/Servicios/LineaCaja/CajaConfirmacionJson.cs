using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace gc.api.core.Servicios.LineaCaja
{
    internal static class CajaConfirmacionJson
    {
        // Campos numericos de los OPENJSON de SPGECO_CAJA_Ope_Confirmar.
        // Los codigos, documentos, fechas y descripciones no se transforman.
        private static readonly HashSet<string> CamposNumericos = new(StringComparer.Ordinal)
        {
            "p_pcosto", "p_pcosto_repo", "in_alicuota", "p_in", "iva_alicuota", "p_iva",
            "po_limite", "p_pneto", "p_margen_imp", "p_margen_vig", "p_pvta",
            "lp_prevision_tot", "lp_prevision_pin", "cantidad_tot", "p_pvta_tot", "bultos",
            "cm_gravado", "cm_no_gravado", "cm_exento", "cm_iva", "cm_ii", "cm_dto",
            "cm_dto_porc", "cmd_cmb_dto", "cmd_cmb_cant", "item",
            "orden", "base", "alicuota", "importe",
            "cm_compte_cuota", "cv_importe", "cv_importe_ori",
            "rb_nro_valor", "rb_opcion_cuota", "rb_importe", "rb_rec", "rb_aux"
        };

        private static readonly HashSet<string> CamposEnteros = new(StringComparer.Ordinal)
        {
            "po_limite", "bultos", "item", "orden", "cm_compte_cuota",
            "rb_nro_valor", "rb_opcion_cuota"
        };

        public static string Normalizar(string json)
        {
            using var texto = new StringReader(json);
            using var lector = new JsonTextReader(texto)
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Decimal
            };
            var contenido = JToken.Load(lector);
            if (contenido is not JArray && contenido is not JObject)
            {
                throw new JsonSerializationException("La confirmacion requiere un objeto o arreglo JSON.");
            }

            IEnumerable<JToken> registros = contenido is JArray lista ? lista.Children() : new[] { contenido };
            foreach (var registro in registros)
            {
                if (registro is not JObject objeto)
                {
                    throw new JsonSerializationException("Cada registro de confirmacion debe ser un objeto JSON.");
                }

                foreach (var propiedad in objeto.Properties())
                {
                    if (!CamposNumericos.Contains(propiedad.Name) || propiedad.Value.Type == JTokenType.Null)
                    {
                        continue;
                    }

                    var valor = propiedad.Value;
                    decimal numero;
                    if (valor.Type == JTokenType.Integer || valor.Type == JTokenType.Float)
                    {
                        numero = valor.Value<decimal>();
                    }
                    else if (valor.Type == JTokenType.String && decimal.TryParse(
                        valor.Value<string>()?.Trim().Replace(',', '.'),
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out numero))
                    {
                        // Algunos DTO legados representan los importes como cadenas.
                    }
                    else
                    {
                        throw new JsonSerializationException($"El campo {propiedad.Name} debe ser numerico.");
                    }

                    numero = Math.Abs(numero);
                    if (CamposEnteros.Contains(propiedad.Name))
                    {
                        if (numero != decimal.Truncate(numero))
                        {
                            throw new JsonSerializationException($"El campo {propiedad.Name} debe ser entero.");
                        }

                        // OPENJSON convierte el texto 1.0 de forma distinta a 1 para int/smallint.
                        propiedad.Value = new JValue(checked((long)numero));
                    }
                    else
                    {
                        propiedad.Value = new JValue(numero);
                    }
                }
            }

            return contenido.ToString(Formatting.None);
        }
    }
}
