namespace gc.sitio.Areas.ControlComun.Models.DetalleDeComprobante
{
	public class DetalleDeCompteModel
	{
		public DetalleDeCompteCabModel Cab { get; set; } = new();
		public DetalleDeCompteIvaModel Iva { get; set; } = new();
		public DetalleDeComptePerModel Per { get; set; } = new();
	}
}
