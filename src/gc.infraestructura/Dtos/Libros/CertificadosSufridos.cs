using System.Globalization;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Libros;

public class CertificadosSufridosFiltro
{
    public string Impuesto { get; set; } = "";
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public bool Fechas { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public bool PorCuit { get; set; }
    public string? Cuit { get; set; }
    public bool SepararCierre { get; set; }
    public bool AgrupadoPorCuit { get; set; }
    public static CertificadosSufridosFiltro Inicial()
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month };
    }
    public void Validar(DateTime? hoy = null)
    {
        var fecha = hoy ?? IvaVentasFiltro.HoyArgentina();
        if (string.IsNullOrWhiteSpace(Impuesto) || Impuesto.Length > 2)
            throw new ArgumentException("Seleccione un impuesto del catálogo de contribuyentes.");
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes is < 1 or > 12)
            throw new ArgumentException("Seleccione año y mes dentro de los últimos seis años.");
        if (Fechas && (!Desde.HasValue || !Hasta.HasValue || Desde.Value.Date > Hasta.Value.Date ||
            Desde.Value.Year != Periodo || Hasta.Value.Year != Periodo || Desde.Value.Month != Mes || Hasta.Value.Month != Mes))
            throw new ArgumentException("Desde y Hasta deben estar en orden y dentro del mes y año seleccionados.");
        if (PorCuit && (string.IsNullOrWhiteSpace(Cuit) || Cuit.Length > 15))
            throw new ArgumentException("Ingrese el CUIT exacto, de hasta 15 caracteres.");
        // El SP termina comparando por igualdad, aunque la primera rama use LIKE.
        if (PorCuit && Cuit!.Any(c => c is '%' or '_' or '[' or ']'))
            throw new ArgumentException("Este procedimiento compara el CUIT exacto al finalizar; no admite patrones de búsqueda.");
        // El SELECT agrupado no devuelve corte_ib_vtas: no es posible separarlo.
        if (Impuesto != "IB" || AgrupadoPorCuit) SepararCierre = false;
    }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public string NombrePdf => $"Certificados_Sufridos_{Impuesto}_{PeriodoTexto}{(AgrupadoPorCuit ? "_por_CUIT" : "")}.pdf";
    public string Descripcion => $"Período {PeriodoTexto}; " +
        (Fechas ? $"fecha de comprobante {Desde:dd/MM/yyyy} a {Hasta:dd/MM/yyyy}; " : "sin rango de fechas; ") +
        $"CUIT: {(PorCuit ? Cuit : "todos")}";
}

public class SufridoImpuesto
{
    public string Id { get; set; } = "";
    public string Texto { get; set; } = "";
    public string Contribuyente { get; set; } = "";
}

// Los 27 alias del SELECT final. No se reconstruyen certificados ni datos fiscales ausentes.
public class CertificadoSufridoFila
{
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? origen { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? imp_id { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? dato { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tco_id { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tco_desc { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_compte { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public DateTime? cm_fecha { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_cuit { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_nombre { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_domicilio { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? @base { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? ali { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? percepcion { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo_desc { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_libro_iva { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? ali_lh { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? percepcion_lh { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? caja_nro_proceso { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public short? caja_nro_cierre { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public short? caja_nro_rend { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public short? rend_item { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public DateTime? carga { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? rend_dato1_valor { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? rend_dato2_valor { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cta_id { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? corte_ib_vtas { get; set; }
}

public class CertificadosSufridosConsulta
{
    [JsonProperty(Required = Required.Always)] public List<CertificadoSufridoFila> Filas { get; set; } = [];
    [JsonProperty(Required = Required.Always)] public List<CertificadoSufridoPorCuitFila> FilasPorCuit { get; set; } = [];
    [JsonProperty(Required = Required.Always)] public bool AgrupadoPorCuit { get; set; }
    [JsonProperty(Required = Required.Always)] public string ImpuestoTexto { get; set; } = "";
    public void ValidarModo(CertificadosSufridosFiltro filtro)
    {
        filtro.Validar();
        if (AgrupadoPorCuit != filtro.AgrupadoPorCuit || (AgrupadoPorCuit ? Filas.Count != 0 : FilasPorCuit.Count != 0))
            throw new InvalidOperationException("La respuesta no corresponde al modo de consulta solicitado.");
    }
    public List<CertificadosSufridosPorCuitGrupo> GruposPorCuit() => FilasPorCuit.GroupBy(f => (f.imp_id, f.tipo, f.tipo_desc))
        .Select(g => new CertificadosSufridosPorCuitGrupo
        {
            Titulo = $"{Texto(g.Key.imp_id)} · {Texto(g.Key.tipo)} · {Texto(g.Key.tipo_desc)}",
            Filas = g.ToList()
        }).ToList();
    public List<CertificadosSufridosGrupo> Grupos(CertificadosSufridosFiltro filtro) => Filas.GroupBy(f =>
        (f.tipo, f.tipo_desc, Corte: filtro.Impuesto == "IB" && filtro.SepararCierre ? f.corte_ib_vtas : null))
        .Select(g => new CertificadosSufridosGrupo
        {
            Titulo = $"{Texto(g.Key.tipo)} · {Texto(g.Key.tipo_desc)}" + (filtro.Impuesto == "IB" && filtro.SepararCierre ? " · " + Corte(g.Key.Corte) : ""),
            Filas = g.ToList()
        }).ToList();
    public static string Corte(string? marca) => marca?.TrimEnd() switch
    {
        "S" => "Carga posterior al cierre de ventas (S)",
        "N" => "Sin marca posterior al cierre de ventas (N)",
        _ => "Corte de ventas no informado"
    };
    public static decimal? Sumar(IEnumerable<CertificadoSufridoFila> filas, Func<CertificadoSufridoFila, decimal?> campo)
    {
        decimal total = 0;
        foreach (var fila in filas) { var valor = campo(fila); if (!valor.HasValue) return null; total += valor.Value; }
        return total;
    }
    public static decimal? TotalFila(CertificadoSufridoFila fila, bool ib) => ib ? fila.percepcion + fila.percepcion_lh : fila.percepcion;
    public static string Numero(decimal? numero) => numero?.ToString("N2", CultureInfo.GetCultureInfo("es-AR")) ?? "No disponible";
    public static string Texto(string? texto) => string.IsNullOrWhiteSpace(texto) ? "—" : texto.TrimEnd();
    public static string Origen(string? origen) => origen?.TrimEnd() switch { "C" => "Compte. Compra", "V" => "Rendición / Certificado", _ => Texto(origen) };
    public static decimal? SumarPorCuit(IEnumerable<CertificadoSufridoPorCuitFila> filas)
    {
        decimal total = 0;
        foreach (var fila in filas) { if (!fila.percepcion.HasValue) return null; total += fila.percepcion.Value; }
        return total;
    }
    public static long? CantidadPorCuit(IEnumerable<CertificadoSufridoPorCuitFila> filas)
    {
        long total = 0;
        foreach (var fila in filas) { if (!fila.percepcion_cant.HasValue) return null; total = checked(total + fila.percepcion_cant.Value); }
        return total;
    }
    public static string Cantidad(long? numero) => numero?.ToString("N0", CultureInfo.GetCultureInfo("es-AR")) ?? "No disponible";
}

public class CertificadosSufridosGrupo
{
    public string Titulo { get; set; } = "";
    public List<CertificadoSufridoFila> Filas { get; set; } = [];
}

// Los ocho alias del SP x_Cuit. percepcion YA incluye LH; no contiene base ni corte.
public class CertificadoSufridoPorCuitFila
{
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? imp_id { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? tipo_desc { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_cuit { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_nombre { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cm_domicilio { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public decimal? percepcion { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public int? percepcion_cant { get; set; }
}

public class CertificadosSufridosPorCuitGrupo
{
    public string Titulo { get; set; } = "";
    public List<CertificadoSufridoPorCuitFila> Filas { get; set; } = [];
}
