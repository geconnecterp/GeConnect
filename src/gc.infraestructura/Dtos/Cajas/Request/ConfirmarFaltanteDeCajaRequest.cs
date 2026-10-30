using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Cajas
{
	public class ConfirmarFaltanteDeCajaRequest : RequestBase
	{
		public string adm_id_fal { get; set; } = string.Empty;
		public DateTime fecha_hasta_fal { get; set; }
		public string json_fal { get; set; } = string.Empty;
	}
}
