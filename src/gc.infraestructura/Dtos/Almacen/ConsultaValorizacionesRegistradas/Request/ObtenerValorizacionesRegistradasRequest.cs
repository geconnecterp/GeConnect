
namespace gc.infraestructura.Dtos.Almacen
{
	public class ObtenerValorizacionesRegistradasRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
