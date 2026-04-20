using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas
{
	public class CargaCtlNuevoItemDetalleRequest : RequestBase
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_rend { get; set; }
		public string tcf_id { get; set; } = string.Empty;
		public bool nuevo_tcf { get; set; }
	}
}
