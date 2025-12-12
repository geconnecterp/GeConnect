using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model
{
	public class BuscarInfoProdMovDModel
	{
		public GridCoreSmart<InfoProdMovStk> GrillaInfoProdMovD { get; set; }
		public SelectList ComboTM { get; set; } = new SelectList(new List<Dto>());
		public SelectList ComboDepositos { get; set; } = new SelectList(new List<Dto>());
		public string selectedValueTM { get; set; }
		public string selectedValueDepos { get; set; }
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
