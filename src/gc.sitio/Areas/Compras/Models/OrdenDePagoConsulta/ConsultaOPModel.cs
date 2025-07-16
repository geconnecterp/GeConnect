using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta
{
	public class ConsultaOPModel
	{
		public GridCoreSmart<OrdenDePagoConsultaDto> GrillaOP { get; set; }
		public decimal Importe { get; set; }
	}
}
