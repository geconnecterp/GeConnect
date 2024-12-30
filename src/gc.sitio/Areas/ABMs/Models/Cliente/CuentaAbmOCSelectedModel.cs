using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaAbmOCSelectedModel
	{
		public SelectList ComboTipoContacto { get; set; }
        public OtroContactoModel OtroContacto { get; set; }
    }
}
