
namespace gc.infraestructura.Dtos.Ventas.Request.Sorteo
{
	public class SorteoCargaListaRequest
	{
		public int Registros { get; set; }
		public int Pagina { get; set; }

		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
