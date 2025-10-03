using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class AgregarItemExtractoModel
	{
		public DateTime Fecha { get; set; }
		public bool Insertar { get; set; } = false;
		public string Comprobante { get; set; } = string.Empty;
		public decimal Debe { get; set; } = 0.00M;
		public decimal Haber { get; set; } = 0.00M;
		public SelectList Movimiento { get; set; }
		public string selected { get; set; } = string.Empty;
		public string abm { get; set; } = "A";
		public int orden { get; set; }
	}
}
