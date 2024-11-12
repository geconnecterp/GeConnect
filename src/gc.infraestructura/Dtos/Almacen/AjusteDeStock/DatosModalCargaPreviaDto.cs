using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class DatosModalCargaPreviaDto : Dto
	{
		public SelectList ComboDepositos { get; set; }
		public SelectList ComboBoxes { get; set; }
        public GridCore<AjustePrevioCargadoDto> ListaProductos { get; set; }

		public DatosModalCargaPreviaDto()
		{
			ListaProductos = new GridCore<AjustePrevioCargadoDto>();
		}
    }
}
