namespace gc.sitio.Areas.Financieros.Models
{
	public class CargaAnticipoModel
	{
		public int cuotas { get; set; }
		public decimal importe { get; set; }
		public decimal intereses { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_desc { get; set; } = string.Empty;
	}
}
