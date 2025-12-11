using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model
{
	public class BuscarInfoProdMovMensModel
	{
		public GridCoreSmart<InfoProdIExMesDto> GrillaInfoProdMovMens { get; set; }
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());
		public string selectedValue { get; set; }
		public int Meses { get; set; } = 12;
	}
}
