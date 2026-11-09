using System.ComponentModel.DataAnnotations;
namespace gc.caja.Models;

public sealed class DocumentoSalidaDto
{
    [Required, RegularExpression("^(FE|CUSTODIA|RECIBO_CD|RECIBO_CC)$")] public string Tipo { get; set; } = "";
    [StringLength(3)] public string? Tco_id { get; set; }
    [StringLength(1)] public string? Tco_letra { get; set; }
    [StringLength(15)] public string? Cm_compte { get; set; }
    [StringLength(15)] public string? Cm_repetido { get; set; }
    [StringLength(20)] public string? Entrega_id { get; set; }
    [StringLength(30)] public string? Rb_compte { get; set; }
}
public sealed class SalidaDiagnosticoDto
{
    public Guid Solicitud { get; set; }
    [Required] public DocumentoSalidaDto Documento { get; set; } = new();
    [Required, RegularExpression("^(PANTALLA|IMPRESORA)$")] public string Modo { get; set; } = "";
    [Required, RegularExpression("^(PREPARANDO|SOLICITANDO|IMPRESION_SOLICITADA|EN_PANTALLA|VERIFICAR)$")] public string Estado { get; set; } = "";
    [StringLength(1000)] public string? Mensaje { get; set; }
}
