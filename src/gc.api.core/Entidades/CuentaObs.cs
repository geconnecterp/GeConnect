
namespace gc.api.core.Entidades
{
	public class CuentaObs : EntidadBase
	{
		public string cta_id { get; set; } = string.Empty;
		public char to_id { get; set; }
		public string cta_obs { get; set; } = string.Empty;
		public string to_desc { get; set; } = string.Empty;
	}
}
