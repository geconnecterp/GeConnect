using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ChequePropioEmitidoModel
	{
		public GridCoreSmart<FinancieroChequePropioEmitidoListaDto> GrillaChequesDetalle { get; set; }
	}
}
