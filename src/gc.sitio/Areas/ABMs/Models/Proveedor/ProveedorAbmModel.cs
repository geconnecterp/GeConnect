using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProveedorAbmModel
	{
        public ProveedorABMDto Proveedor { get; set; }

		public ProveedorAbmModel()
		{
			Proveedor = new ProveedorABMDto();
		}
    }
}
