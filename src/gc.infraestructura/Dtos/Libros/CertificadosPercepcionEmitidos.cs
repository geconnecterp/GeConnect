using System.Globalization;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Libros;

public class CertificadosPercepcionFiltro
{
    public string Impuesto { get; set; } = "";
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public bool Fechas { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public bool PorCuit { get; set; }
    public string? Cuit { get; set; }
    public static CertificadosPercepcionFiltro Inicial()
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month };
    }
    public void Validar(DateTime? hoy = null)
    {
        var fecha = hoy ?? IvaVentasFiltro.HoyArgentina();
        if (Impuesto is not ("IB" or "IV")) throw new ArgumentException("Seleccione un impuesto implementado: IB o IV.");
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes is < 1 or > 12)
            throw new ArgumentException("Seleccione año y mes dentro de los últimos seis años.");
        if (Fechas && (!Desde.HasValue || !Hasta.HasValue || Desde.Value.Date > Hasta.Value.Date ||
            Desde.Value.Year != Periodo || Hasta.Value.Year != Periodo || Desde.Value.Month != Mes || Hasta.Value.Month != Mes))
            throw new ArgumentException("Desde y Hasta deben estar en orden y dentro del mes y año seleccionados.");
        if (PorCuit && (string.IsNullOrWhiteSpace(Cuit) || Cuit.Length > 15))
            throw new ArgumentException("Ingrese un CUIT o patrón de hasta 15 caracteres.");
    }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public string Descripcion => $"Libro IVA {PeriodoTexto}; " +
        (Fechas ? $"fecha de comprobante {Desde:dd/MM/yyyy} a {Hasta:dd/MM/yyyy}; " : "sin rango de fechas; ") +
        $"CUIT: {(PorCuit ? Cuit : "todos")}";
}

public class PercepcionImpuesto
{
    public string Id { get; set; } = "";
    public string Texto { get; set; } = "";
    public string AgentePer { get; set; } = "";
    public bool Soportado => Id is "IB" or "IV";
}

// Los dieciséis alias del SELECT AP, sin reconstruir comprobantes ni completar NULL.
public class CertificadoPercepcionFila
{
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tco { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? compte { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? @base { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? ali { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? percepcion { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? percepcion_lh { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? razon_social { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? domicilio { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cuit { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? ib_nro { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public DateTime? fecha { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? letra { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo_geco { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo_desc { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tco_id_ori { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_compte_ori { get; set; }
}

public class CertificadosPercepcionConsulta
{
    [JsonProperty(Required = Required.Always)] public List<CertificadoPercepcionFila> Filas { get; set; } = [];
    public decimal? TotalBase => Total(f => f.@base);
    public decimal? TotalPercepcion => Total(f => f.percepcion);
    public decimal? TotalLH => Total(f => f.percepcion_lh);
    public bool ImportesIncompletos => Filas.Any(f => !f.@base.HasValue || !f.percepcion.HasValue);
    private decimal? Total(Func<CertificadoPercepcionFila, decimal?> campo) => Filas.Any(f => !campo(f).HasValue) ? null : Filas.Sum(f => campo(f)!.Value);
    public static string Importe(decimal? n) => n?.ToString("N2", CultureInfo.GetCultureInfo("es-AR")) ?? "No disponible";
}
