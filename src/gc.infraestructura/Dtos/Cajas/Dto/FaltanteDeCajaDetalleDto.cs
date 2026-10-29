
namespace gc.infraestructura.Dtos.Cajas
{
	public class FaltanteDeCajaDetalleDto : Dto
	{
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public DateTime dia { get; set; }
		public string caja_nro_proceso { get; set; } = string.Empty;
		public string caja_nro_cierre { get; set; } = string.Empty;
		public decimal faltante { get; set; }
		public decimal sobrante { get; set; }
		public decimal acumulado { get; set; }
		public int semana { get; set; }
		public char procesado { get; set; }
	}
}
