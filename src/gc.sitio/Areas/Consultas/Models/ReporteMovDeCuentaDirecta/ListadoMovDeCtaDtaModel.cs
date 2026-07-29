using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ListadoMovDeCtaDtaModel
	{
		public GridCoreSmart<MovimientoListaDto> GrillaProducto { get; set; }
		// Leyenda final ya armada
		public string Leyenda { get; set; } = string.Empty;
		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaFechas { get; set; } = string.Empty;
		public string LeyendaCuentas { get; set; } = string.Empty;
	}
}
