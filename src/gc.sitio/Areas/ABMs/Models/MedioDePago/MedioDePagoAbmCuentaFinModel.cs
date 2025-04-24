using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class MedioDePagoAbmCuentaFinModel
	{
        public CuentaFinModel CuentaFin { get; set; }
		public SelectList ComboTipo { get; set; }
		public SelectList ComboAdministracion { get; set; }
		public SelectList ComboCuentaGasto { get; set; }
		public SelectList ComboCuentaContable { get; set; }
		public GridCoreSmart<FinancieroListaDto> ListaCuentaFin { get; set; }
		public MedioDePagoAbmCuentaFinModel()
		{ 
			CuentaFin = new CuentaFinModel();
		}
	}
}
