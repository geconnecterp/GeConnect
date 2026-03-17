using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class OrdenDeRepartoCambioPrecioModel
	{
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<CambioDePrecioDto> ListaCambioPrecios { get; set; }
	}
}
