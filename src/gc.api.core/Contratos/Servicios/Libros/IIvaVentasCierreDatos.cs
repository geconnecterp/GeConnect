using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaVentasCierreDatos
{
    Task<IvaVentasCierreEstado> Consultar(CancellationToken ct);
    Task<IvaVentasCierreRespuesta> Cerrar(string periodoEsperado, string usuario, string sucursal, CancellationToken ct);
    Task<bool> ExisteCierre(string periodo, CancellationToken ct);
    IAsyncEnumerable<string?> LineasRg3685(string periodo, char tipo, CancellationToken ct);
}
