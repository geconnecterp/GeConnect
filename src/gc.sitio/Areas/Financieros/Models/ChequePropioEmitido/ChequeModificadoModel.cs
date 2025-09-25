namespace gc.sitio.Areas.Financieros.Models
{
	public class ChequeModificadoModel
	{
		public string ModificadoPor { get; set; } = string.Empty;
		public string NroCheque { get; set; } = string.Empty;
		public DateTime Fecha { get; set; }
		public string ANombreDe { get; set; } = string.Empty;
	}
}
