using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IBancoServicio : IServicio<BancoDto>
	{
		List<BancoDto> GetBancoParaABM(string ctafId, string token);
	}
}
