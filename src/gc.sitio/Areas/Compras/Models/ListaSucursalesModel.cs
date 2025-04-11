using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaSucursalesModel
	{
		public string adm_id { get; set; }
		public SelectList ComboSucursales { get; set; }
	}
}
