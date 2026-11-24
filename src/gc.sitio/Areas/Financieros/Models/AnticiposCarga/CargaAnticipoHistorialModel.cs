using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CargaAnticipoHistorialModel
	{
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public GridCoreSmart<AnticipoFinanEmpListaDto> GrillaAnticipoFinanEmp { get; set; }
		public GridCoreSmart<AnticipoDetalleDto> GrillaAnticipoFinanEmpDetalle { get; set; }
	}
}
