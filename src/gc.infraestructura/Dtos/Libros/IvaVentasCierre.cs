using System.Globalization;

namespace gc.infraestructura.Dtos.Libros;

public class IvaVentasCierrePedido
{
    public string Periodo { get; set; } = "";
    public void Validar() => IvaVentasCierrePeriodo.Validar(Periodo);
}

public static class IvaVentasCierrePeriodo
{
    public static void Validar(string? periodo)
    {
        if (periodo?.Length != 6 || periodo.Any(c => c < '0' || c > '9') ||
            !DateTime.TryParseExact(periodo, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new ArgumentException("El período debe contener un año y mes válidos (AAAAMM).");
    }
    public static string Texto(string periodo) { Validar(periodo); return periodo[..4] + "-" + periodo[4..]; }
}

// Campos exactos de comptes_vtas_iva_mensual, retornados por SPGECO_IVA_V_Libros.
public class IvaVentasCierreFila
{
    public string tipo_registro { get; set; } = "";
    public string periodo { get; set; } = "";
    public decimal importe { get; set; }
    public decimal importe_gravado { get; set; }
    public decimal importe_no_gravado { get; set; }
    public decimal importe_exento { get; set; }
    public decimal importe_iva { get; set; }
    public decimal percep_imp_nacionales { get; set; }
    public decimal percep_ib { get; set; }
    public decimal percep_municipales { get; set; }
    public decimal importe_imp_internos { get; set; }
    public DateTime fecha_cierre { get; set; }
    public decimal[] Importes => [importe_no_gravado, importe_exento, importe_gravado, importe_iva,
        importe_imp_internos, percep_ib, percep_imp_nacionales, percep_municipales, importe];
    public static string Numero(decimal value) => value.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));
}

public class IvaVentasCierreEstado
{
    public string ProximoPeriodo { get; set; } = "";
    public bool PuedeCerrar { get; set; }
    public List<IvaVentasCierreFila> Libros { get; set; } = [];
    public void Validar()
    {
        try
        {
            IvaVentasCierrePeriodo.Validar(ProximoPeriodo);
            foreach (var fila in Libros) IvaVentasCierrePeriodo.Validar(fila.periodo);
        }
        catch (ArgumentException ex) { throw new InvalidOperationException("SQL devolvió un período de cierre inválido.", ex); }
        if (Libros.Count > 72 || Libros.Any(f => string.IsNullOrWhiteSpace(f.tipo_registro)) ||
            Libros.Select(f => (f.tipo_registro, f.periodo)).Distinct().Count() != Libros.Count)
            throw new InvalidOperationException("El historial no cumple el contrato del listado de cierres.");
    }
}

public class IvaVentasCierreRespuesta
{
    public short resultado { get; set; }
    public string resultado_msj { get; set; } = "";
}

public class IvaVentasCierreConflictoException(string mensaje) : InvalidOperationException(mensaje);
