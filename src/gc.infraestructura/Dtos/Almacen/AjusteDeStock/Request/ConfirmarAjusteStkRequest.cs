
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request
{
	public class ConfirmarAjusteStkRequest
	{
		public string json { get; set; } = string.Empty;
		public string admId { get; set; } = string.Empty;
		public string usuId { get; set; } = string.Empty;
		public string compteOri { get; set; } = string.Empty;
	}
}
