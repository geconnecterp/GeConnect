using gc.infraestructura.Dtos.Almacen.Request;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class ComprobanteModel
	{
		public SelectList listaTiposComptes { get; set; }
		public SelectList listaCondAfip { get; set; }
		public SelectList listaCuentaDirecta { get; set; }
		public SelectList listaMotivos { get; set; }
		public AgregarOPDRequest itemOPD { get; set; }
	}
}
