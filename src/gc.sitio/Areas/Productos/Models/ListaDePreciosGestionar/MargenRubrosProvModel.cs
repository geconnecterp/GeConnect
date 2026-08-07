using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Productos.Models.ListaDePreciosGestionar
{
	public class MargenRubrosProvModel
	{
		public SelectList ListaSectores { get; set; }
		public string SectorSeleccionado { get; set; } = string.Empty;
		public SelectList ListaRubros { get; set; }
		public string RubroSeleccionado { get; set; } = string.Empty;
		public SelectList ListaProveedores { get; set; }
		public string ProveedorSeleccionado { get; set; } = string.Empty;
		public bool CargarPorSector { get; set; } = true;
		public decimal Mgn { get; set; } = 0.000M;
	}
}
