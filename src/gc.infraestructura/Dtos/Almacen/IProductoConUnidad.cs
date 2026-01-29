
namespace gc.infraestructura.Dtos.Almacen
{
	public interface IProductoConUnidad
	{
		string up_id { get; }
		string up_tipo { get; }
		bool PermiteDecimales { get; }
	}
}
