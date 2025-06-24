namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class EdicionTipoEmisionChequesModel : EdicionTipoModel
	{
		public string NroCheque { get; set; } = string.Empty;
		public bool Automatico { get; set; } = false;
		public DateTime Fecha { get; set; } = DateTime.Now;
		public string ANombreDe { get; set; } = string.Empty;
		public decimal Importe { get; set; } = 0.00M;
		public string ImporteS { get; set; } = string.Empty;
	}
}
