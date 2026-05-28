using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request
{
	public class CargarDevolucionesRequest : QueryFilters
	{
		
		public DateTime fecha_d { get; set; }
		public DateTime fecha_h { get; set; }
		public bool adm { get; set; }
		public string adm_list { get; set; } = string.Empty;
		public bool cta { get; set; }
		public string cta_list { get; set; } = string.Empty;
	}
}
