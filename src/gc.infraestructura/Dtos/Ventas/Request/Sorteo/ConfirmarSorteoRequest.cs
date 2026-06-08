using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas
{
	public class ConfirmarSorteoRequest : RequestBase
	{
		public string abm { get; set; } = string.Empty;
		public string so_sorteo { get; set; } = string.Empty;
		public string so_desc { get; set; } = string.Empty;
		public DateTime so_desde { get; set; }
		public DateTime so_hasta { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public char? so_participan { get; set; }
		public char? so_inclusion_tipo { get; set; }
		public decimal? so_inclusion_valor { get; set; }
		public char? so_inclusion_acumula { get; set; }
		public string json_p { get; set; } = string.Empty;
		public string json_a { get; set; } = string.Empty;
	}
}
