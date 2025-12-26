
namespace gc.infraestructura.Dtos
{
	public class ProductoEnCierreDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal planillas_conteo1 { get; set; } = 0.000M;
		public decimal planillas_conteo2 { get; set; } = 0.000M;
		public decimal ps_conteo { get; set; } = 0.000M;
		public char ps_ajuste { get; set; }
		public bool seleccionar { get; set; }
	}
}
