using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProveedorServicio : IServicio<ProveedorDto>
    {
		List<ProveedorABMDto> GetProveedorParaABM(string ctaId, string token);
	}
}
