namespace gc.infraestructura.Dtos.Libros;

// Contrato controlado entre las APIs y el sitio. Nunca incluye el valor fiscal recibido.
public sealed class IvaComprasDatoInvalidoException(string campo) : InvalidOperationException(
    $"No se pudo completar la operación de Libro IVA: el campo {campo} contiene un dato con formato inválido. " +
    "Solicite su revisión al administrador. No se muestran resultados ni se entrega un Excel parcial.")
{
    public const string Codigo = "IVA_COMPRAS_DATO_INVALIDO";
    public string Campo { get; } = campo;
}
