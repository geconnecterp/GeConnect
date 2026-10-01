using System.ComponentModel.DataAnnotations;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.caja.Models.Estacion;

// Sólo configuración del puesto; nunca credenciales, permisos o datos de Caja obtenidos del SP.
public sealed class EstacionPuesto
{
    [Required, StringLength(80), RegularExpression(@"[A-Za-z0-9_.-]+")]
    public string EstacionId { get; set; } = "";
    [Required, StringLength(4), RegularExpression(@"[A-Za-z0-9]+")]
    public string CajaId { get; set; } = "";
    [Required, StringLength(10), RegularExpression(@"[A-Za-z0-9]+")]
    public string AdmId { get; set; } = "";
    [Required, StringLength(200), RegularExpression(@"[^\r\n]+")]
    public string IP { get; set; } = "";
    [Range(1, 2)] public int Facturacion { get; set; }
    [Range(0, 2)] public int TipoCnnCF { get; set; }
    public bool Acumula { get; set; }

    public CajaSettings Configuracion() => new() { CajaId = CajaId, AdmId = AdmId, IP = IP,
        Facturacion = (TipoFact)Facturacion, TipoCnnCF = (TipoCnnCF)TipoCnnCF, acumula = Acumula };
}
