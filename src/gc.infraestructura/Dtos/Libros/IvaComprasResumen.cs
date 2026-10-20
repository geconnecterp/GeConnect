using System.Globalization;

namespace gc.infraestructura.Dtos.Libros;

public class IvaComprasResumenFiltro
{
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public static IvaComprasResumenFiltro Inicial()
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month };
    }
    public void Validar(DateTime? hoy = null)
    {
        var fecha = hoy ?? IvaVentasFiltro.HoyArgentina();
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes is < 1 or > 12)
            throw new ArgumentException("Seleccione año y mes dentro de los últimos seis años.");
    }
}

public class IvaComprasResumenControl
{
    public string tipo { get; set; } = "";
    public string ccb { get; set; } = "";
    public string ccb_desc { get; set; } = "";
    public decimal importe { get; set; }
    public decimal importe_iva { get; set; }
    public string Concepto => string.IsNullOrEmpty(ccb) ? ccb_desc : $"{ccb} · {ccb_desc}";
}

public class IvaComprasResumenConcepto
{
    public string tipo { get; set; } = "";
    public string tipo_desc { get; set; } = "";
    public decimal importe { get; set; }
}

public class IvaComprasResumenBloque
{
    public string Codigo { get; set; } = "";
    public string Nombre => Codigo == "BI" ? "Bienes de intercambio" : "Servicios / Otros";
    [Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
    public List<IvaComprasResumenControl> Control { get; set; } = [];
    [Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
    public List<IvaComprasResumenConcepto> Resumen { get; set; } = [];
    public decimal TotalContabilidad => Control.Sum(f => f.importe);
    public decimal TotalLibro => Control.Sum(f => f.importe_iva);
    public decimal Diferencia => TotalContabilidad - TotalLibro;
    public decimal TotalResumen => Resumen.Sum(f => f.importe);
}

// No agrupar por tipo: IV se repite para Crédito Fiscal y NC; cada alícuota tiene su GR.
public class IvaComprasResumenLibro
{
    public IvaComprasResumenFiltro Filtro { get; set; } = new();
    [Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
    public IvaComprasResumenBloque Bienes { get; set; } = new() { Codigo = "BI" };
    [Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
    public IvaComprasResumenBloque Otros { get; set; } = new() { Codigo = "OT" };
    public int Cantidad => Bienes.Control.Count + Bienes.Resumen.Count + Otros.Control.Count + Otros.Resumen.Count;
    public bool TodoCero => Bloques.All(b => b.Control.All(f => f.importe == 0 && f.importe_iva == 0) && b.Resumen.All(f => f.importe == 0));
    [Newtonsoft.Json.JsonIgnore]
    public IEnumerable<IvaComprasResumenBloque> Bloques => [Bienes, Otros];
    public static string Numero(decimal valor) => valor.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));
    public void Validar()
    {
        if (Filtro == null || Bienes == null || Otros == null || Bienes.Codigo != "BI" || Otros.Codigo != "OT" ||
            Bloques.Any(b => b.Control == null || b.Resumen == null || b.Control.Any(f => f == null) || b.Resumen.Any(f => f == null)))
            throw new InvalidOperationException("El resumen recibido no contiene los cuatro conjuntos válidos.");
        foreach (var b in Bloques)
        {
            _ = b.TotalContabilidad; _ = b.TotalLibro; _ = b.Diferencia; _ = b.TotalResumen;
        }
    }
}
