using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Consultas.Models
{
	public class DetalleMesPVModel
	{
		public GridCoreSmart<AnaValDeVtaDetPVDto> GrillaDetPV { get; set; }
		public string Leyenda { get; set; }
	}
}
