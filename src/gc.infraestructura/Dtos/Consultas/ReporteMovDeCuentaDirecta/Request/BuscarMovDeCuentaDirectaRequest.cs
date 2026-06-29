
namespace gc.infraestructura.Dtos.Consultas
{
	public class BuscarMovDeCuentaDirectaRequest
	{
		public List<string> ctag_list { get; set; } = [];
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
