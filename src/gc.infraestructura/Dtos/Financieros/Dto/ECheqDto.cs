
namespace gc.infraestructura.Dtos.Financieros
{
	public class ECheqDto : Dto
	{
		public string json { get; set; } = string.Empty;
		public string formato_salida { get; set; } = string.Empty;
		public bool encabezado { get; set; }
	}
}
