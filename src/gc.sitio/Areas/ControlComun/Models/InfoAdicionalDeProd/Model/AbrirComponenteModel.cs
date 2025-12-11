using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model
{
	public class AbrirComponenteModel
	{
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());
	}
}
