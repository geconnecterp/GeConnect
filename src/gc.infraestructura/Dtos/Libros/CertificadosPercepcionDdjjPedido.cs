namespace gc.infraestructura.Dtos.Libros;

// DDJJ mensual: no admite filtros de fecha/CUIT ni paginación de la consulta.
public class CertificadosPercepcionDdjjPedido
{
    public string Impuesto { get; set; } = "";
    public int Periodo { get; set; }
    public int Mes { get; set; }
    public string PeriodoTexto => $"{Periodo:0000}-{Mes:00}";
    public void Validar() => new CertificadosPercepcionFiltro { Impuesto = Impuesto, Periodo = Periodo, Mes = Mes }.Validar();
    public string Nombre => $"DDJJ_Percepciones_{Impuesto}_{PeriodoTexto}.xlsx";
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
