
namespace gc.infraestructura.Dtos.OrdenDePago.Dtos
{
	public class OPDirectaObligacionesDto : Dto
	{
		public string op_compte { get; set; } = string.Empty;
		public string concepto { get; set; } = string.Empty;
		public DateTime fecha_vencimiento { get; set; }
		public string gasto { get; set; } = string.Empty;
		public string motivo { get; set; } = string.Empty;
		public decimal imputado { get; set; } = 0.00M;

		///Campos que son clave de los registros
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
	}
}
