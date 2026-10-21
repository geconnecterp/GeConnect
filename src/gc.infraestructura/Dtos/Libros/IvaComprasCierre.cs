using System.Globalization;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Libros;

public class IvaComprasCierrePedido
{
    public string Periodo { get; set; } = "";
    public void Validar() => IvaComprasCierrePeriodo.Validar(Periodo);
}

public class IvaComprasCierreDescargaPedido : IvaComprasCierrePedido
{
    public string TipoRegistro { get; set; } = "";
    public new void Validar()
    {
        base.Validar();
        if (TipoRegistro?.Length != 1 || char.IsWhiteSpace(TipoRegistro[0]) || char.IsControl(TipoRegistro[0]))
            throw new ArgumentException("Seleccione un registro válido del historial.");
    }
}

public static class IvaComprasCierrePeriodo
{
    public static void Validar(string? periodo)
    {
        if (periodo?.Length != 6 || periodo.Any(c => c < '0' || c > '9') ||
            !DateTime.TryParseExact(periodo, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new ArgumentException("El período debe contener un año y mes válidos (AAAAMM).");
    }
    public static string Texto(string periodo) { Validar(periodo); return periodo[..4] + "-" + periodo[4..]; }
}

// Campos exactos de comptes_comp_iva_mensual, retornados por SPGECO_IVA_C_Libros.
public class IvaComprasCierreFila
{
    [JsonProperty(Required = Required.Always)] public string tipo_registro { get; set; } = "";
    [JsonProperty(Required = Required.Always)] public string periodo { get; set; } = "";
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public int? cant_registros { get; set; }
    [JsonProperty(Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)] public string? cuit { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_gravado { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_no_gravado { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_exento { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_iva { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal percep_imp_nacionales { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal percep_ib { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal percep_municipales { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_imp_internos { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_otro_ng { get; set; }
    [JsonProperty(Required = Required.Always)] public decimal importe_mt { get; set; }
    [JsonIgnore] public decimal[] Importes => [importe_no_gravado, importe_exento, importe_gravado, importe_iva,
        importe_imp_internos, percep_ib, percep_imp_nacionales, percep_municipales, importe_otro_ng, importe_mt, importe];
    public static string Numero(decimal value) => value.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));
}

public class IvaComprasCierreEstado
{
    [JsonProperty(Required = Required.Always)] public string ProximoPeriodo { get; set; } = "";
    [JsonProperty(Required = Required.Always)] public bool PuedeCerrar { get; set; }
    [JsonProperty(Required = Required.Always)] public List<IvaComprasCierreFila> Libros { get; set; } = [];
    public void Validar()
    {
        try
        {
            IvaComprasCierrePeriodo.Validar(ProximoPeriodo);
            if (Libros == null || Libros.Any(f => f == null)) throw new ArgumentException("Historial incompleto.");
            foreach (var fila in Libros) IvaComprasCierrePeriodo.Validar(fila.periodo);
        }
        catch (ArgumentException ex) { throw new InvalidOperationException("SQL devolvió un período de cierre inválido.", ex); }
        if (Libros.Count > 72 || Libros.Any(f => f.tipo_registro?.Length != 1 || char.IsWhiteSpace(f.tipo_registro[0]) || char.IsControl(f.tipo_registro[0])) ||
            Libros.Select(f => (f.tipo_registro, f.periodo)).Distinct().Count() != Libros.Count)
            throw new InvalidOperationException("El historial no cumple el contrato del listado de cierres.");
    }
}

public class IvaComprasCierreRespuesta
{
    [JsonProperty(Required = Required.Always)] public short resultado { get; set; }
    [JsonProperty(Required = Required.Always)] public string resultado_msj { get; set; } = "";
}

public class IvaComprasCierreConflictoException(string mensaje) : InvalidOperationException(mensaje);

public class IvaComprasCierreArchivoException(string mensaje) : InvalidOperationException(mensaje);

