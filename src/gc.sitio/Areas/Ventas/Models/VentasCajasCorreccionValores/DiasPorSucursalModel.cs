using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	public class DiasPorSucursalModel
	{
		public SelectList ListaDias { get; set; }
		public string DiaSeleccionado { get; set; } = string.Empty;
	}
}
