using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ModalIvaModel
	{
		public bool EsGravado { get; set; } = false;
		public ConceptoFacturadoDto ConceptoFacturado { get; set; }
		public SelectList IvaSituacionLista { get; set; }
		public SelectList IvaAlicuotaLista { get; set; }
	}
}
