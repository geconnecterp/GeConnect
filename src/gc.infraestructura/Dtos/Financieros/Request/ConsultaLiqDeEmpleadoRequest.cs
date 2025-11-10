using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ConsultaLiqDeEmpleadoRequest : BaseFilters
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public int? Registros { get; set; }
		public int? Pagina { get; set; }
	}
}
