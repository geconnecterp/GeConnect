using gc.infraestructura.Dtos.CuentaComercial;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaDirectaAbmModel
	{
		public CuentaGastoDto CuentaDirecta { get; set; }
		public SelectList TipoCuenta { get; set; }
		public SelectList CuentaContable { get; set; }
		public CuentaDirectaAbmModel()
		{
			CuentaDirecta = new CuentaGastoDto();
		}
	}
}
