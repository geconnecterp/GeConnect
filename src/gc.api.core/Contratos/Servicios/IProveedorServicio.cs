using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
namespace gc.api.core.Contratos.Servicios
{
    public interface IProveedorServicio : IServicio<Proveedor>
    {
		List<ProveedorABMDto> GetProveedorParaABM(string cta_id);
	}
}
