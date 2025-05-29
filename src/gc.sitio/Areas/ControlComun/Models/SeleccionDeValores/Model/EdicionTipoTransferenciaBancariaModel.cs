namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class EdicionTipoTransferenciaBancariaModel : EdicionTipoModel
	{
		public string NroTransferencia { get; set; } = string.Empty;
		public DateTime Fecha { get; set; } = DateTime.Now;
		public string Concepto { get; set; } = string.Empty;
		public decimal Importe { get; set; } = 0.00M;
	}
}
