using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ProveedoresFamiliaModel
	{
		public SelectList ListaFamilias { get; set; } = new SelectList(new List<Dto>());
		public string FamiliaSeleccionada { get; set; } = string.Empty;
	}
}
