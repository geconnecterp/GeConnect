using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Deposito;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IDepositoServicio:IServicio<DepositoDto>
    {
        Task<List<DepositoInfoBoxDto>> BuscarBoxLibres(string depo_id, bool soloLibres, string tokenCookie);
        Task<List<DepositoInfoStkDto>> BuscarDepositoInfoStk(string depo_id, string token);
        Task<List<DepositoInfoStkValDto>> BuscarDepositoInfoStkVal(string adm_id, string depo_id, string concepto, string token);
        List<DepositoDto> ObtenerDepositosDeAdministracion(string adm_id,string token);
        Task<List<RemitoGenDto>> ObtenerRemitos(string admId, string token);
        Task<List<DepositoInfoBoxDto>> BuscarBoxPorDeposito(string depoId, string token);
        Task<List<DepositoInfoBoxDto>> ObtenerInfoDeBox(string boxId, string token);
	}
}
