
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroActuRegsPorConciPrevManualRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public List<int> itemsExtractoMarcados { get; set; } = [];
		public List<int> itemsSistemaMarcados { get; set; } = [];
	}
}
