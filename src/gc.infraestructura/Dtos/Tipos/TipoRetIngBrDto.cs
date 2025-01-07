
namespace gc.infraestructura.Dtos
{
	public class TipoRetIngBrDto : Dto
	{
        public string rib_id { get; set; } = string.Empty;
        public string rib_desc { get; set; } = string.Empty;
		public string rib_lista { get; set; } = string.Empty;
		public decimal rib_alic { get; set; } = 0.000M;
        public decimal rib_alic_lh { get; set; } = 0.000M;
		public char rib_tipo_base { get; set; }
        public decimal rib_min_imponible { get; set; } = 0.000M;
	}
}
