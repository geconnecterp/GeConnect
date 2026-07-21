using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.SolAuth.Comando;

public class CrearSolicitudAutorizacionComando
{
    [Required]
    [StringLength(50)]
    public string CodigoModuloOrigen { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string usu_id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string IdSolicitudExterna { get; set; } = string.Empty;

    [Range(1, short.MaxValue)]
    public int DerCodigo { get; set; }

    [Range(1, int.MaxValue)]
    public int TimeoutSegundos { get; set; }

    [Required]
    public ResolucionPorDefectoComando ResolucionPorDefecto { get; set; } = new();
    public JToken? Contexto { get; set; } // Will be serialized to JSON
}

public class ResolucionPorDefectoComando
{
    [Required]
    [RegularExpression("^(?i:APROBADO|RECHAZADO)$")]
    public string Decision { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CodigoResolucion { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Mensaje { get; set; }
}
