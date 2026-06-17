
namespace gc.infraestructura.Dtos
{
	public class RepoVtaRequest
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
	}

	public class RepoVtaDetRequest : RepoVtaRequest
	{
		public string tcf_id { get; set; } = string.Empty;
	}
}
