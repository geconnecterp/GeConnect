using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas
{
	public class AnularCtlArqueoRequest : RequestBase
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_rend { get; set; }
		public string tcf_id { get; set; } = string.Empty;
	}
}
