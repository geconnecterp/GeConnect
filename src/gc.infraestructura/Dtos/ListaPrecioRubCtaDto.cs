
namespace gc.infraestructura.Dtos
{
	public class ListaPrecioRubCtaDto : Dto
	{
		public string lp_id { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public decimal lpp_mgn_principal_porc { get; set; }
	}
}
