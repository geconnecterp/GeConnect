
namespace gc.infraestructura.Dtos.Cajas
{
	public class FaltanteDeCajaDto : Dto
	{
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public DateTime dia { get; set; }
		public decimal acumulado { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
	}
}
