using System.Globalization;

namespace gc.pocket.site.Helpers;

public static class CantidadProductoFormatter
{
    private static readonly CultureInfo CulturaNumerica = CultureInfo.InvariantCulture;

    public static bool PermiteDecimales(string? upId)
    {
        return !string.Equals(upId?.PadLeft(2, '0'), "07", StringComparison.Ordinal);
    }

    public static bool PermiteDecimales(int upId)
    {
        return upId != 7;
    }

    public static string Formatear(decimal valor, string? upId)
    {
        return valor.ToString(PermiteDecimales(upId) ? "#,##0.000" : "#,##0", CulturaNumerica);
    }

    public static string Formatear(decimal valor, int upId)
    {
        return valor.ToString(PermiteDecimales(upId) ? "#,##0.000" : "#,##0", CulturaNumerica);
    }

    public static string FormatearTotalMixto(decimal valor)
    {
        return valor.ToString("#,##0.###", CulturaNumerica);
    }
}
