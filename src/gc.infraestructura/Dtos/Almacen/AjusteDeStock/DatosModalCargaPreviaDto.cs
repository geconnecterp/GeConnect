using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class DatosModalCargaPreviaDto : Dto
	{
		public SelectList ComboDepositos { get; set; } = new SelectList(new List<SelectListItem>());
		public SelectList ComboBoxes { get; set; } = new SelectList(new List<SelectListItem>());		
        public GridCoreSmart<AjustePrevioCargadoDto> ListaProductos { get; set; }

		public DatosModalCargaPreviaDto()
		{
			ListaProductos = new GridCoreSmart<AjustePrevioCargadoDto>();
		}
    }
}
