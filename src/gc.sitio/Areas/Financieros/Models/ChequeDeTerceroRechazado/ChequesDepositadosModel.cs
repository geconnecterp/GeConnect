using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ChequesDepositadosModel
	{
		public GridCoreSmart<FinancieroChequeDepositadoDto> GrillaChequesDepositados { get; set; }
		public DateTime FechaRechazado { get; set; }
	}
}
