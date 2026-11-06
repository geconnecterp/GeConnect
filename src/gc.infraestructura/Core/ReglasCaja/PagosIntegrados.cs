using gc.infraestructura.Dtos.Cajas;
namespace gc.infraestructura.Core.ReglasCaja;

public static class PagosIntegrados
{
    public static bool EsMedio(string? medio) => medio?.Trim().ToUpperInvariant() is "T1" or "T2";
    public static decimal Recargo(decimal monto, decimal porcentaje) => decimal.Round(monto * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
    public static decimal TotalAjustes(IEnumerable<Json_Valor> valores, IReadOnlyDictionary<string, string> catalogo) =>
        valores.Where(v => EsMedio(catalogo.GetValueOrDefault(v.ins_id))).Sum(v => v.rb_aux);
}
