using gc.infraestructura.Dtos.Libros;

namespace gc.sitio.core.Servicios.Contratos.Libros
{
    public interface IBalanceGrServicio
    {
        Task<List<BalanseGrDto>> ObtenerBalanceGeneral(int eje_nro,string token);
    }
}
