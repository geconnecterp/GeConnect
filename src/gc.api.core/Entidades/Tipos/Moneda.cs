
namespace gc.api.core.Entidades
{
	public class Moneda : EntidadBase
	{
		public string Mon_Codigo { get; set; } = string.Empty;
		public string Mon_Desc { get; set; } = string.Empty;
		public string Mon_Lista { get; set; } = string.Empty;
		public char Mon_Vigente { get; set; }
		public decimal Mon_Cotizacion { get; set; } = 0.00M;
		public char Mon_Defecto { get; set; }
	}
}
