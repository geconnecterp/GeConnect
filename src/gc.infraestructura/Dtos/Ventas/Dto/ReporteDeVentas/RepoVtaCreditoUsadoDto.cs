
namespace gc.infraestructura.Dtos
{
	public class RepoVtaCreditoUsadoDto : Dto
	{
		public string cta_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public decimal cc_importe { get; set; }
		public string cta_denominacion { get; set; } = string.Empty;
		public string rb_compte { get; set; } = string.Empty;
	}
}
