using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros;

public interface IIvaComprasCierreDatos
{
    Task<IvaComprasCierreEstado> Consultar(CancellationToken ct);
    Task<IvaComprasCierreRespuesta> Cerrar(string periodoEsperado, string usuario, string sucursal, CancellationToken ct);
    Task<bool> ExisteCierre(string periodo, string tipoRegistro, CancellationToken ct);
    IAsyncEnumerable<string?> LineasRg3685(string periodo, char tipo, CancellationToken ct);
}

