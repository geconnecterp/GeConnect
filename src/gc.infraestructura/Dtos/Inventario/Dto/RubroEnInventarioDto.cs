
namespace gc.infraestructura.Dtos
{
	public class RubroEnInventarioDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public string inv_descripcion { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public int cant_prod_stk { get; set; } = 0;
		public int cant_prod_stk_positivo { get; set; } = 0;
		public int cant_prod_conteo { get; set; } = 0;
	}
}
