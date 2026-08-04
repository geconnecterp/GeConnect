using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas
{
	public class ReporteEvalDeNivelDeServicioModel
	{
		public SelectList ListaSucursales { get; set; }
		public SelectList ListaDepositos { get; set; }
		public SelectList ListaFamilias { get; set; }
		public SelectList ListaRubros { get; set; }
		public SelectList ListaAgrupadores { get; set; }

		// Valores seleccionados
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public string DepositoSeleccionado { get; set; } = string.Empty;
		public string FamiliaSeleccionada { get; set; } = string.Empty;
		public string RubroSeleccionado { get; set; } = string.Empty;
		public string AgrupadorSeleccionado { get; set; } = "0";
	}
}
