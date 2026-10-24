namespace gc.infraestructura.Dtos.Libros;

// Contrato mensual: deliberadamente no recibe CUIT, fechas ni paginación.
public class CertificadosRetencionDdjjPedido
{
    public string Impuesto { get; set; } = "";
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public void Validar() => new CertificadosRetencionFiltro { Impuesto = Impuesto, Periodo = Periodo, Mes = Mes }.Validar();
    public string Nombre => $"DDJJ_Retenciones_{Impuesto}_{PeriodoTexto}." + (Impuesto == "GA" ? "txt" : "xlsx");
    public string ContentType => Impuesto == "GA" ? "text/plain; charset=utf-8" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
