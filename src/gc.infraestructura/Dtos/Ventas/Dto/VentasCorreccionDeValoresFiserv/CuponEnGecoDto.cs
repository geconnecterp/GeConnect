
namespace gc.infraestructura.Dtos.Ventas
{
	public class CuponEnGecoDto : Dto
	{
		public string id { get; set; } = string.Empty;
		public string descripcion { get; set; } = string.Empty;
		public string tc_tra { get; set; } = string.Empty;
		public string lote { get; set; } = string.Empty;
		public string cupon { get; set; } = string.Empty;
		public decimal importe { get; set; }
		public string e { get; set; } = string.Empty;
	}
}
