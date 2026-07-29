
namespace gc.infraestructura.Dtos.Consultas
{
	public class BuscarMovDeCuentaDirectaRequest
	{
		public List<string> ctag_list { get; set; } = [];
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public string ctag_list_textos { get; set; } = string.Empty;
	}
}
