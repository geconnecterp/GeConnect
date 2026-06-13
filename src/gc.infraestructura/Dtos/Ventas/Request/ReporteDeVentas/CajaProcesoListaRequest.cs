using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas.Request
{
	public class CajaProcesoListaRequest : BaseFilters
	{
		public int Registros { get; set; }
		public int Pagina { get; set; }

		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public string adm_list { get; set; }
	}
}
