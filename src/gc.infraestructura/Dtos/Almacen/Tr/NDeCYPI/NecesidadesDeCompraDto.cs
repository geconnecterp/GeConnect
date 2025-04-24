using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI
{
	public class NecesidadesDeCompraDto : Dto
	{
		public SelectList ComboProveedores { get; set; } = new SelectList(new List<Dto>());	
        public SelectList ComboProveedoresFamilia { get; set; } = new SelectList(new List<Dto>());
        public SelectList ComboRubros { get; set; } = new SelectList(new List<Dto>());
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());	
		public GridCoreSmart<ProductoNCPIDto> Productos { get; set; }
		public bool ProductosSimilares { get; set; }
		public bool ProductosDelMismoProveedor { get; set; } = true;
		public NecesidadesDeCompraDto()
		{
			Productos = new GridCoreSmart<ProductoNCPIDto>();
		}
	}
}
