
namespace gc.infraestructura.Dtos.Ventas
{
	public class CuponesNoEncontradosDto : Dto
	{
		public DateTime fecha { get; set; }
		public string id { get; set; } = string.Empty;
		public string producto { get; set; } = string.Empty;
		public string nro_trans { get; set; } = string.Empty;
		public string nro_term { get; set; } = string.Empty;
		public string tc_tra { get; set; } = string.Empty;
		public string lote { get; set; } = string.Empty;
		public string cupon { get; set; } = string.Empty;
		public decimal importe { get; set; }
		public string proceso_cierre { get; set; } = string.Empty;
	}
}
