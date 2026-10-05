using System.ComponentModel.DataAnnotations;
namespace gc.caja.Models;
public sealed class RecuperarDetalleRequest
{
    [Required, MinLength(1), MaxLength(2000)]
    public List<ProductoRespaldo> Productos { get; set; } = [];
}
public sealed class ProductoRespaldo
{
    [Required, StringLength(50)] public string Codigo { get; set; } = "";
    // cantidad_tot ya está expresada en unidades, no multiplicar nuevamente por bultos.
    [Range(typeof(decimal), "0.000001", "999999999")] public decimal Cantidad { get; set; }
}
