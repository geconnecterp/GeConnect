
namespace gc.api.core.Entidades
{
	public class TipoGasto : EntidadBase
	{
		public string ctag_id { get; set; } = string.Empty;
        public string ctag_denominacion { get; set; } = string.Empty;
		public string ctag_lista { get; set; } = string.Empty;
		public char ctag_gasto_ingreso { get; set; }
        public string ctag_tipo { get; set; } = string.Empty;
	}
}
