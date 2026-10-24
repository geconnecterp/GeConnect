using System.Globalization;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Libros;

public class CertificadosRetencionFiltro
{
    public string Impuesto { get; set; } = "";
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public bool Fechas { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public bool PorCuit { get; set; }
    public string? Cuit { get; set; }
    public static CertificadosRetencionFiltro Inicial()
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month };
    }
    public void Validar(DateTime? hoy = null)
    {
        var fecha = hoy ?? IvaVentasFiltro.HoyArgentina();
        if (Impuesto is not ("IB" or "GA" or "IV"))
            throw new ArgumentException("Seleccione un impuesto implementado: IB, GA o IV.");
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes is < 1 or > 12)
            throw new ArgumentException("Seleccione año y mes dentro de los últimos seis años.");
        if (Fechas && (!Desde.HasValue || !Hasta.HasValue || Desde.Value.Date > Hasta.Value.Date ||
            Desde.Value.Year != Periodo || Hasta.Value.Year != Periodo || Desde.Value.Month != Mes || Hasta.Value.Month != Mes))
            throw new ArgumentException("Desde y Hasta deben estar en orden y dentro del mes y año seleccionados.");
        // Sólo longitud del contrato SQL. Se permite el patrón LIKE sin validar dígito verificador.
        if (PorCuit && (string.IsNullOrWhiteSpace(Cuit) || Cuit.Length > 15))
            throw new ArgumentException("Ingrese un CUIT o patrón de hasta 15 caracteres.");
    }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public string Descripcion => $"Período {PeriodoTexto}; " +
        (Fechas ? $"fechas {Desde:dd/MM/yyyy} a {Hasta:dd/MM/yyyy}; " : "sin rango de fechas; ") +
        $"CUIT: {(PorCuit ? Cuit : "todos")}";
}

public class RetencionImpuesto
{
    public string Id { get; set; } = "";
    public string Texto { get; set; } = "";
    public string AgenteRet { get; set; } = "";
    public bool Soportado => Id is "IB" or "GA" or "IV";
}

// Contrato exacto del SELECT #repo. Los NULL se preservan y nunca se convierten a cero.
public class CertificadoRetencionFila
{
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? certificado { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cta_id { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cuit { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? raz_soc { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? domicilio { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public DateTime? fecha { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? @base { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? ali { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? reten { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? estado { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? op_compte { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cta_denominacion { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? ib_nro { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? cib_ali_lh { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? cib_reten_lh { get; set; }
}

public class CertificadosRetencionConsulta
{
    [JsonProperty(Required = Required.Always)] public List<CertificadoRetencionFila> Filas { get; set; } = [];
    public decimal? TotalBase => Total(f => f.@base);
    public decimal? TotalRetencion => Total(f => f.reten);
    public decimal? TotalLH => Total(f => f.cib_reten_lh);
    public bool ImportesIncompletos => Filas.Any(f => !f.@base.HasValue || !f.reten.HasValue);
    private decimal? Total(Func<CertificadoRetencionFila, decimal?> campo) => Filas.Any(f => !campo(f).HasValue) ? null : Filas.Sum(f => campo(f)!.Value);
    public static string Importe(decimal? n) => n?.ToString("N2", CultureInfo.GetCultureInfo("es-AR")) ?? "No disponible";
}
