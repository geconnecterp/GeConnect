
namespace gc.infraestructura.Dtos
{
	public class InventarioValorizacionDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public string inv_descripcion { get; set; } = string.Empty;
		public int cant_prod_stk { get; set; }
		public int cant_prod_stk_positivo { get; set; }
		public int cant_prod_conteo { get; set; }
	}

	public class InventarioValorizacionRubroDto : InventarioValorizacionDto
	{
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
	}

	public class  InventarioValorizacionBoxDto : InventarioValorizacionDto
	{
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
	}
}
