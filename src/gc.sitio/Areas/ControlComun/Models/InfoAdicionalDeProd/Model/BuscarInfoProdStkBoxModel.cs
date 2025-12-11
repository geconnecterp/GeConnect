using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models
{
	public class BuscarInfoProdStkBoxModel
	{
		public GridCoreSmart<InfoProdStkBox> GrillaInfoProdStkBox { get; set; }
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());
		public string selectedValue { get; set; }
	}
}
