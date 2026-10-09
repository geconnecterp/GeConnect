using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.Areas.Mstk.Models.PlanillaDeElaboracionFraccion
{
	public class ProductoModel : Dto, IProductoConUnidad
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal cantidad { get; set; }
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}
}
