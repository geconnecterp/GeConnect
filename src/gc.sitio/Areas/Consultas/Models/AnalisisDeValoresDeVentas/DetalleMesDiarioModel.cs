using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Consultas.Models
{
	public class DetalleMesDiarioModel
	{
		public GridCoreSmart<AnaValDeVtaDetDiarioDto> GrillaDetDiario { get; set; }
		public string Leyenda { get; set; }
	}
}
