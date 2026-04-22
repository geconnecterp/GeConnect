using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas
{
	public class ConfirmacionContableRequest : RequestBase
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
	}
}
