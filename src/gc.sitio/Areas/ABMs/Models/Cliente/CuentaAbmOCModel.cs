using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaAbmOCModel
	{
		public SelectList ComboTipoContacto { get; set; }
		public GridCoreSmart<CuentaContactoDto> CuentaOtrosContactos { get; set; }
        public OtroContactoModel OtroContacto { get; set; }

		public CuentaAbmOCModel()
		{
			OtroContacto = new OtroContactoModel();
		}
	}
}
