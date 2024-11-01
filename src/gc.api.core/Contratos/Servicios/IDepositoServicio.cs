using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Deposito;

namespace gc.api.core.Contratos.Servicios
{
    public interface IDepositoServicio : IServicio<Deposito>
    {
        List<Deposito> ObtenerDepositosDeAdministracion(string adm_id);
        List<DepositoInfoBoxDto> ObtenerDepositioInfoBox(string adm_id, bool soloLibre);
        List<DepositoInfoStkDto> ObtenerDepositoInfoStk(string depo_id);
        List<DepositoInfoStkValDto> ObtenerDepositoInfoStkValorizado(string adm_id, string depo_id, string concepto);
    }
}
