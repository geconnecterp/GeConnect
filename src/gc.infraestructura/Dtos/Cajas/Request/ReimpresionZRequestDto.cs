using System.Globalization;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class ReimpresionZRangoDto
    {
        public bool? PorFecha { get; set; }
        public string Desde { get; set; } = string.Empty;
        public string Hasta { get; set; } = string.Empty;

        public static DateTime Hoy => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Argentina Standard Time").Date;
        public static bool ControladorSoportado(string? ctrlId) => ctrlId?.Trim() == "50";

        // La interfaz/API usa ISO; únicamente en el límite con SQL se transforma a AAMMDD.
        public bool Validar(DateTime hoy, out string desdeSP, out string hastaSP, out string error)
        {
            desdeSP = hastaSP = error = string.Empty;
            if (!PorFecha.HasValue) { error = "Seleccione búsqueda por número o por fecha."; return false; }
            if (PorFecha.Value)
            {
                if (!DateTime.TryParseExact(Desde, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var desde) ||
                    !DateTime.TryParseExact(Hasta, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hasta))
                { error = "Ingrese fechas válidas en Desde y Hasta."; return false; }
                if (desde > hasta) { error = "Desde no puede ser posterior a Hasta."; return false; }
                if ((hasta - desde).Days > 7) { error = "El intervalo no puede superar 7 días."; return false; }
                if (hasta > hoy.Date) { error = "Hasta no puede ser posterior a hoy."; return false; }
                if (hasta < hoy.Date.AddYears(-5)) { error = "Hasta no puede ser anterior a cinco años atrás."; return false; }
                desdeSP = desde.ToString("yyMMdd", CultureInfo.InvariantCulture);
                hastaSP = hasta.ToString("yyMMdd", CultureInfo.InvariantCulture);
            }
            else
            {
                static bool Numero(string valor, out int numero)
                {
                    numero = 0;
                    return !string.IsNullOrEmpty(valor) && valor.Length <= 5 && valor.All(c => c >= '0' && c <= '9') &&
                        int.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out numero) && numero >= 1;
                }
                if (!Numero(Desde, out var desde) || !Numero(Hasta, out var hasta))
                { error = "Ingrese números enteros entre 1 y 99999."; return false; }
                if (desde > hasta) { error = "Desde no puede ser mayor que Hasta."; return false; }
                if (hasta - desde > 5) { error = "La diferencia entre Hasta y Desde no puede superar 5."; return false; }
                desdeSP = desde.ToString(CultureInfo.InvariantCulture);
                hastaSP = hasta.ToString(CultureInfo.InvariantCulture);
            }
            return true;
        }
    }

    public class ReimpresionZRequestDto : ReimpresionZRangoDto
    {
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
    }
}
