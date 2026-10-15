using System.Globalization;

namespace gc.infraestructura.Dtos.Libros;

public class IvaVentasFiltro
{
    public const int MaxPagina = 500;
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public bool Fechas { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public bool PorCuit { get; set; }
    public string? Cuit { get; set; }
    public bool PorSucursal { get; set; }
    public string[] Sucursales { get; set; } = [];
    public bool PorComprobante { get; set; }
    public string[] Comprobantes { get; set; } = [];
    public bool PorCaja { get; set; }
    public string[] Cajas { get; set; } = [];
    public int Pagina { get; set; } = 1;
    public int Registros { get; set; } = 100;

    public static DateTime HoyArgentina() => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Argentina Standard Time").Date;

    public static IvaVentasFiltro Inicial(int registros = 100)
    {
        var hoy = HoyArgentina();
        return new() { Periodo = hoy.Year, Mes = hoy.Month, Registros = Math.Clamp(registros > 0 ? registros : 100, 1, MaxPagina) };
    }

    // La exportación ignora exclusivamente la navegación, nunca los filtros.
    public void Validar(bool exportacion = false, DateTime? hoy = null)
    {
        var fecha = hoy ?? HoyArgentina();
        if (Periodo < fecha.Year - 5 || Periodo > fecha.Year || Mes < 1 || Mes > 12)
            throw new ArgumentException("Seleccione un período dentro de los últimos seis años.");
        if (Fechas && (!Desde.HasValue || !Hasta.HasValue || Desde.Value.Date > Hasta.Value.Date ||
            Desde.Value.Year != Periodo || Hasta.Value.Year != Periodo || Desde.Value.Month != Mes || Hasta.Value.Month != Mes))
            throw new ArgumentException("Desde y Hasta deben pertenecer al mes y año seleccionados y estar en orden.");
        // Se controla solamente el tamaño del contrato SQL; no formato ni dígito verificador.
        if (PorCuit && (string.IsNullOrWhiteSpace(Cuit) || Cuit.Length > 15))
            throw new ArgumentException("Ingrese el CUIT o patrón a buscar (hasta 15 caracteres).");
        ValidarLista(PorSucursal, Sucursales, 4, "sucursal");
        ValidarLista(PorComprobante, Comprobantes, 3, "tipo de comprobante");
        ValidarLista(PorCaja, Cajas, 4, "punto de venta");
        if (!exportacion && (Registros < 1 || Registros > MaxPagina || Pagina < 1 || (long)Registros * (Pagina - 1) > int.MaxValue))
            throw new ArgumentException("La página debe ser positiva y contener entre 1 y 500 registros.");
    }

    private static void ValidarLista(bool activa, string[]? valores, int largo, string nombre)
    {
        if (!activa) return;
        if (valores == null || valores.Length == 0 || valores.Length > 2000 ||
            valores.Any(x => string.IsNullOrWhiteSpace(x) || x.Length > largo || x.Contains(',') || x.Contains('%')))
            throw new ArgumentException($"Seleccione al menos un {nombre} válido de la lista.");
    }

    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public string Descripcion => $"Período {PeriodoTexto}; " +
        (Fechas ? $"fechas {Desde:dd/MM/yyyy} a {Hasta:dd/MM/yyyy}; " : "sin rango de fechas; ") +
        $"sucursales: {(PorSucursal ? string.Join(",", Sucursales) : "activas")}; " +
        $"comprobantes: {(PorComprobante ? string.Join(",", Comprobantes) : "todos")}; " +
        $"cajas: {(PorCaja ? string.Join(",", Cajas) : "todas")}; " +
        $"CUIT: {(PorCuit ? Cuit : "todos")}";
}
