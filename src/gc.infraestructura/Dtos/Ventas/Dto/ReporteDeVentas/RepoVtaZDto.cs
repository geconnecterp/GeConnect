
namespace gc.infraestructura.Dtos
{
	public class RepoVtaZDto : Dto
	{
		public int orden { get; set; }
		public string tipo { get; set; } = string.Empty;
		public string tipo_desc { get; set; } = string.Empty;
		public decimal ali { get; set; }
		public decimal ft_a_imp { get; set; }
		public decimal ft_b_imp { get; set; }
		public decimal nc_a_imp { get; set; }
		public decimal nc_b_imp { get; set; }
		public decimal nd_a_imp { get; set; }

	}
}
