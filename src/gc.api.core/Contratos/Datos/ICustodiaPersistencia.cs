using gc.infraestructura.Dtos.Cajas;

namespace gc.api.core.Interfaces.Datos;

public record CustodiaCambio(CustodiaRendicionDto Antes, CustodiaLineaDto Despues);
public record CustodiaSpResultado(short Codigo, string Mensaje, string Id = "");
public interface ICustodiaPersistencia
{
    List<CustodiaRendicionDto> Pendientes(string sucursal, string instrumento);
    CustodiaSpResultado Corregir(IReadOnlyList<CustodiaCambio> cambios, string sucursal, string usuario, Guid solicitud);
}
public sealed class CustodiaPersistenciaException(string mensaje, bool incierto, Exception inner) : Exception(mensaje, inner)
{
    public bool Incierto { get; } = incierto;
}
