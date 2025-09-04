using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class VencimientoChequeEmitidoModel
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		//TODO Marce: Armar el Dto de las grillas, deje esas para que no de error al compilar
		public GridCoreSmart<FinancieroChequeDepositadoDto> GrillaCheques { get; set; }
		public GridCoreSmart<FinancieroChequeDepositadoDto> GrillaChequesDetalle { get; set; }
		public decimal Total { get; set; }
	}
}
