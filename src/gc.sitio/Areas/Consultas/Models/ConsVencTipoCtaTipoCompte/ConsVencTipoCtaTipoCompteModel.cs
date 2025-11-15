using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ConsVencTipoCtaTipoCompteModel
	{
		public DateTime FechaVencDesde { get; set; }
		public DateTime FechaVencHasta { get; set; }
		public DateTime FechaGenDesde { get; set; }
		public DateTime FechaGenHasta { get; set; }
		public string selectedValue { get; set; } = string.Empty;
		public SelectList ListaTipoClientes { get; set; }
		public SelectList ListaTipoProveedores { get; set; }
		public SelectList ListaTipoCompte { get; set; }
	}
}
