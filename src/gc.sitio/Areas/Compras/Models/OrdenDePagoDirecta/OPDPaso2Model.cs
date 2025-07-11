using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class OPDPaso2Model
	{
		public GridCoreSmart<OPDirectaObligacionesDto> GrillaObligaciones { get; set; }
		public GridCoreSmart<ValoresDesdeObligYCredDto> GrillaValores { get; set; }
	}
}
