using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI
{
	public class NecesidadesDeCompraDto : Dto
	{
		public SelectList ComboProveedores { get; set; }
		public SelectList ComboProveedoresFamilia { get; set; }
		public SelectList ComboRubros { get; set; }
		public GridCore<ProductoNCPIDto> Productos { get; set; }
        public bool ProductosSimilares { get; set; }

        public NecesidadesDeCompraDto()
		{
			Productos = new GridCore<ProductoNCPIDto>();
		}
	}
}
