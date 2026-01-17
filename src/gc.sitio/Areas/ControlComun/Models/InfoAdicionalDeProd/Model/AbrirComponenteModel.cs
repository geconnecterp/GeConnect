using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model
{
	public class AbrirComponenteModel
	{
		public SelectList ComboSucursales { get; set; } = new SelectList(new List<Dto>());
		public string pId { get; set; } = string.Empty;
		public string pDesc { get; set; } = string.Empty;
		public string productoFull => $"({pId}) {pDesc}";
		public string ctaId { get; set; } = string.Empty;
		public string ctaDesc { get; set; } = string.Empty;
		public string ctaFull => $"({ctaId}) {ctaDesc}";

	}
}
