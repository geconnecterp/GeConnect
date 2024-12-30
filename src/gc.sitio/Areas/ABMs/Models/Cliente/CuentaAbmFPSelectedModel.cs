using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaAbmFPSelectedModel
	{
		public SelectList ComboFormasDePago { get; set; }
		public SelectList ComboTipoCuentaBco { get; set; }
        public FormaDePagoModel FormaDePago { get; set; }
    }
}
