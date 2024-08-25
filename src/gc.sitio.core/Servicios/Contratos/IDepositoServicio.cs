using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IDepositoServicio:IServicio<DepositoDto>
    {
        List<DepositoDto> ObtenerDepositosDeAdministracion(string adm_id,string token);
    }
}
