using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InformacionDeBoxModel
	{
		public SelectList ListaDepositos { get; set; }
		public string DepositoSeleccionado { get; set; } = string.Empty;
		public bool Gondola { get; set; } = false;
		public string GondolaValue { get; set; } = string.Empty;
		public bool Nivel { get; set; } = false;
		public string NivelValue { get; set; } = string.Empty;
		public bool Rack { get; set; } = false;
		public string RackValue { get; set; } = string.Empty;
		public bool Zona { get; set; } = false;
		public string ZonaValue { get; set; } = string.Empty;
		public bool SoloLibres { get; set; } = false;
	}
}
