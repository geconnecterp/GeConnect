using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Inventario
{
	public class ConfirmarInventarioRequest : RequestBase
	{
		public string abm { get; set; } = string.Empty;
		public string inv_nro { get; set; } = string.Empty;
		public string invt_id { get; set; } = string.Empty;
		public string inv_descripcion { get; set; } = string.Empty;
		public DateTime inv_apertura { get; set; }
		public DateTime inv_cierre { get; set; }
		public string depo_id { get; set; } = string.Empty;
		public string json_r { get; set; } = string.Empty;
		public string json_u { get; set; } = string.Empty;
	}
}
