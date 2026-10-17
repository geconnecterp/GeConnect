using System.Globalization;

namespace gc.infraestructura.Dtos.Libros;

public class IvaVentasResumenFiltro
{
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public static IvaVentasResumenFiltro Inicial()
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month };
    }
    public void Validar(DateTime? hoy = null)
    {
        var fecha = hoy ?? IvaVentasFiltro.HoyArgentina();
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes < 1 || Mes > 12)
            throw new ArgumentException("Seleccione año y mes dentro de los últimos seis años.");
    }
}

// Contrato exacto del SP: no transformar signos, descripciones ni alícuotas residuales.
public class IvaVentasResumenFila
{
    public string adm_id { get; set; } = "";
    public string adm_nombre { get; set; } = "";
    public string tipo_ope { get; set; } = "";
    public short orden { get; set; }
    public string tipo { get; set; } = "";
    public string tipo_desc { get; set; } = "";
    public decimal? ali { get; set; }
    public decimal fa_mt_imp { get; set; }
    public decimal fa_cf_imp { get; set; }
    public decimal fa_ri_imp { get; set; }
    public decimal fa_ex_imp { get; set; }
    public decimal fa_ot_imp { get; set; }
    public decimal Total => fa_mt_imp + fa_cf_imp + fa_ri_imp + fa_ex_imp + fa_ot_imp;
    public string Concepto => $"({(tipo_ope == "V" ? "FV" : tipo_ope == "C" ? "NC" : tipo_ope)}) {tipo_desc}";
    public decimal[] Importes => [fa_mt_imp, fa_cf_imp, fa_ri_imp, fa_ex_imp, fa_ot_imp, Total];
}

public class IvaVentasResumenGrupo
{
    public string Id { get; init; } = "";
    public string Nombre { get; init; } = "";
    public bool EsTotal => Id == "9999";
    public List<IvaVentasResumenFila> Filas { get; init; } = [];
    public decimal[] Totales => Enumerable.Range(0, 6).Select(c => Filas.Sum(f => f.Importes[c])).ToArray();
}

public class IvaVentasResumenLibro
{
    public IvaVentasResumenFiltro Filtro { get; init; } = new();
    public List<IvaVentasResumenGrupo> Grupos { get; init; } = [];
    public int Cantidad => Grupos.Sum(g => g.Filas.Count);
    public bool TodoCero => Grupos.All(g => g.Filas.All(f => f.Importes.All(i => i == 0)));
    public static readonly string[] Columnas = ["Concepto", "Vtas. MT", "Vtas. CF", "Vtas. RI", "Vtas. EX", "Vtas. Ot.", "Total"];
    public static string Numero(decimal valor) => valor.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));

    public static IvaVentasResumenLibro Crear(IvaVentasResumenFiltro filtro, IReadOnlyList<IvaVentasResumenFila> filas)
    {
        // Fallar antes de mostrar/exportar un resultado parcial o un contrato incompleto.
        if (filas.Count > 0 && !filas.Any(f => f.adm_id.Trim() == "9999"))
            throw new InvalidOperationException("La consulta no devolvió el grupo Total Libro (9999).");
        if (filas.Any(f => string.IsNullOrWhiteSpace(f.adm_id) || f.tipo_ope is not ("V" or "C")))
            throw new InvalidOperationException("La consulta devolvió una sucursal u operación no reconocida.");
        var grupos = filas.GroupBy(f => f.adm_id.Trim())
            .OrderBy(g => g.Key == "9999").ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new IvaVentasResumenGrupo
            {
                Id = g.Key, Nombre = g.Key == "9999" ? "Total Libro" : g.First().adm_nombre.Trim(),
                Filas = g.OrderBy(f => f.tipo_ope == "C").ThenBy(f => f.orden).ThenBy(f => f.ali)
                    .ThenBy(f => f.tipo, StringComparer.Ordinal).ThenBy(f => f.tipo_desc, StringComparer.Ordinal).ToList()
            }).ToList();
        // Verificar también que las sumas pueden representarse antes de emitir una respuesta.
        foreach (var grupo in grupos) _ = grupo.Totales;
        return new() { Filtro = filtro, Grupos = grupos };
    }
}
