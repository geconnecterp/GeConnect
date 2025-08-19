namespace gc.sitio.Areas.Financieros.Models.PresentacionDeValoresEnCartera
{
	public class DetalleDePresentacionRequest
	{
		public decimal totalSeleccionadoEnCartera { get; set; } = 0.00M;
		public decimal saldoDeCtaf { get; set; } = 0.00M;
		public string ctafIdSelected { get; set; } = string.Empty;
		public string ctafDescSelected { get; set; } = string.Empty;
		public string ctafIdLista { get; set; } = string.Empty; //Strings separados por '|'
		public string tcfIdSelected { get; set; } = string.Empty;
	}
}
