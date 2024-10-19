using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IDepositoServicio:IServicio<DepositoDto>
    {
        List<DepositoDto> ObtenerDepositosDeAdministracion(string adm_id,string token);
        Task<List<RemitoGenDto>> ObtenerRemitos(string admId, string token);
    }
}
