
namespace gc.infraestructura.Dtos
{
	public class ComisionesDeRepartidoresResumenDto : Dto
	{
		public string rp_id { get; set; } = string.Empty;
		public string rp_nombre { get; set; } = string.Empty;
		public decimal comi_fac { get; set; }
		public decimal comi_nc { get; set; }
		public decimal comi_base_fac { get; set; }
		public decimal comi_base_nc { get; set; }
	}
}
