using gc.infraestructura.Dtos.Libros;

namespace gc.api.core.Contratos.Servicios.Libros
{
    public interface IApiBalanceGeneralServicio
    {
        List<BalanseGrDto> ObtenerBalanceGeneral(int eje_nro);
    }
}
