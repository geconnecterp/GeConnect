using gc.api.core.Entidades;

namespace gc.api.core.Contratos.Servicios
{
    public interface IDepositoServicio : IServicio<Deposito>
    {
        List<Deposito> ObtenerDepositosDeAdministracion(string adm_id);
    }
}
