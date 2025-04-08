using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ModalOtroConceptoModel
	{
		public OtroTributoDto OtroTributo { get; set; }
		public SelectList OtrasPercepcionesLista { get; set; }
	}
}
