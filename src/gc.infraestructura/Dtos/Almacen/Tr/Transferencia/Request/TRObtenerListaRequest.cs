
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia.Request
{
	public class TRObtenerListaRequest
	{
		public string tit_id { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_id_des { get; set; } = string.Empty;
	}
}
