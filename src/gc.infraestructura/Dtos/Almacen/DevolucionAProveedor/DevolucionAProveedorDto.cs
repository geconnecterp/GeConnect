
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	public class DevolucionAProveedorDto : Dto
	{
		public SelectList ComboDepositos { get; set; }
		public SelectList ComboBoxes { get; set; }
        public GridCore<ProductoADevolverDto> ProductosADevolver { get; set; }
		public DevolucionAProveedorDto()
		{
			ProductosADevolver = new GridCore<ProductoADevolverDto>();
		}
	}
}
