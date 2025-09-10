using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class VencimientoChequeEmitidoModel
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public GridCoreSmart<FinancieroBcoVencChequeEmitidoDto> GrillaCheques { get; set; }
		public GridCoreSmart<FinancieroBcoVencChequeEmitidoListaDto> GrillaChequesDetalle { get; set; }
		public decimal Total { get; set; }
	}
}
