
namespace gc.infraestructura.Dtos
{
	public class SorteoCargaListaDto : Dto
	{
		public int total_registros { get; set; } = 0;
		public int total_paginas { get; set; } = 0;
		public string so_sorteo { get; set; } = string.Empty;
		public string so_desc { get; set; } = string.Empty;
		public DateTime so_desde { get; set; }
		public DateTime so_hasta { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public char? so_participan { get; set; }
		public char? so_participan_forma { get; set; }
		public char? so_inclusion_acumula { get; set; }
		public char? so_inclusion_tipo { get; set; }
		public decimal? so_inclusion_valor { get; set; }
		public char so_actu { get; set; }
	}
}
