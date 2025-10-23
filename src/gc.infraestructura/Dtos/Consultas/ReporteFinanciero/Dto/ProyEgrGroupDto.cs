
namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class ProyEgrGroupDto : Dto
	{
		public DateTime fecha { get; set; }
		public decimal prevision_egresos { get; set; } = 0.00M;
		public decimal prevision_acumulada { get; set; } = 0.00M;
	}
}
