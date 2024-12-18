using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaAbmFPModel
	{
		public SelectList ComboFormasDePago { get; set; }
		public SelectList ComboTipoCuentaBco { get; set; }
		public GridCore<CuentaFPDto> CuentaFormasDePago { get; set; }
	}
}
