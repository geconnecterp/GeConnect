
namespace gc.infraestructura.Dtos
{
	public class ImprimirULDto : Dto
	{
		public string tipo { get; set; } = string.Empty;
		public string tipo_id { get; set; } = string.Empty;
		public string motivo { get; set; } = string.Empty;
		public string ul_id { get; set; } = string.Empty;
		public DateTime hoy { get; set; }
	}
}
