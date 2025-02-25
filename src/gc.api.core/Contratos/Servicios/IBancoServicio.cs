using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface IBancoServicio : IServicio<Banco>
	{
		List<BancoDto> GetBancoParaABM(string ctaf_id);
	}
}
