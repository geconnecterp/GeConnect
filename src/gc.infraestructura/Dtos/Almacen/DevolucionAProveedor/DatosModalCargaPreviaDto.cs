using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	public class DatosModalCargaPreviaDPDto : Dto
	{
		public SelectList ComboDepositos { get; set; }
		public SelectList ComboBoxes { get; set; }
        public GridCore<DevolucionPrevioCargadoDto> ListaProductos { get; set; }

		public DatosModalCargaPreviaDPDto()
		{
			ListaProductos = new GridCore<DevolucionPrevioCargadoDto>();
		}
    }
}
