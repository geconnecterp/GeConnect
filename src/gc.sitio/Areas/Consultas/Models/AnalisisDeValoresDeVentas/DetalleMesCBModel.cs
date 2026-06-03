using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Consultas.Models
{
	public class DetalleMesCBModel
	{
		public GridCoreSmart<AnaValDeVtaDetCBDto> GrillaDetCB { get; set; }
		public string Leyenda { get; set; }
	}
}
