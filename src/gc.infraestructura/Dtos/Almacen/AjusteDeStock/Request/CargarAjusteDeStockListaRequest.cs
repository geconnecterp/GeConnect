using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request
{
	public class CargarAjusteDeStockListaRequest : QueryFilters
	{
		public DateTime fecha_d { get; set; }
		public DateTime fecha_h { get; set; }
		public bool adm { get; set; }
		public string adm_list { get; set; } = string.Empty;
	}
}
