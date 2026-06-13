using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models.ReporteDeVentas
{
	public class ProcesosDeCajaModel
	{
		public GridCoreSmart<CajaProcesoListaDto> ListaProcesos { get; set; }
		public GridCoreSmart<CajaProcesoListaDto> ListaCierres { get; set; } //Reemplazar por la lista de Cierres de caja
	}
}
