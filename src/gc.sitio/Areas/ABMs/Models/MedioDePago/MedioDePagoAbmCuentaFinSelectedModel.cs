using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class MedioDePagoAbmCuentaFinSelectedModel
	{
        public CuentaFinModel CuentaFin { get; set; }
        public SelectList ComboTipo { get; set; }
		public SelectList ComboAdministracion { get; set; }
		public SelectList ComboCuentaGasto { get; set; }
		public SelectList ComboCuentaContable { get; set; }
	}
}
