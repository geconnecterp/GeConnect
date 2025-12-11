using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model
{
	public class BuscarInfoProdMovSemModel
	{
		public GridCoreSmart<InfoProdIExSemanaDto> GrillaInfoProdMovSem { get; set; }
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());
		public string selectedValue { get; set; }
		public int Semanas { get; set; } = 4;
	}
}
