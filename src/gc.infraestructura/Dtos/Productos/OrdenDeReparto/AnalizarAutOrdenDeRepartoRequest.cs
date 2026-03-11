
namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class AnalizarAutOrdenDeRepartoRequest
	{
		public string or_compte { get; set; } = string.Empty;
		public string dep_ids { get; set; } = string.Empty;
		public bool stk_existente { get; set; } = false;
		public bool sustituto { get; set; } = false;
		public int palet_nro { get; set; } = 0;
	}
}
