using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	public class DatosModalCargaPreviaDPDto : Dto
	{
		public SelectList ComboDepositos { get; set; } = new SelectList(new List<SelectListItem>());
		public SelectList ComboBoxes { get; set; } = new SelectList(new List<SelectListItem>());	
        public GridCoreSmart<DevolucionPrevioCargadoDto> ListaProductos { get; set; }

		public DatosModalCargaPreviaDPDto()
		{
			ListaProductos = new GridCoreSmart<DevolucionPrevioCargadoDto>();
		}
    }
}
