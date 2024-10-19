using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;

namespace gc.api.core.Contratos.Servicios
{
    public interface IDepositoServicio : IServicio<Deposito>
    {
        List<Deposito> ObtenerDepositosDeAdministracion(string adm_id);        
    }
}
