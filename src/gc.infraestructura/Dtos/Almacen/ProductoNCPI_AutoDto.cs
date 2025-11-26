
namespace gc.infraestructura.Dtos.Almacen
{
	public class ProductoNCPI_AutoDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public int p_unidad_pres { get; set; }
		public decimal vtas { get; set; } = 0.00M;
		public int vtas_dias { get; set; } = 0;
		public decimal vtas_diarias { get; set; } = 0.00M;
		public decimal stk { get; set; } = 0.00M;
		public decimal stk_min { get; set; } = 0.00M;
		public decimal stk_max { get; set; } = 0.00M;
		public int stk_dias { get; set; } = 0;
		public int? stk_dias_pond { get; set; } = 0;
		public int pendiente { get; set; } = 0;
		public int auto_cantidad { get; set; } = 0;
		public int auto_bulto { get; set; } = 0;
		public int up_cantidad { get; set; } = 0;
		public int up_bulto { get; set; } = 0;
	}

	public class ProductoNCPI_AutoDto_ : Dto
	{
		public string p_id { get; set; } = string.Empty;
	}
}
