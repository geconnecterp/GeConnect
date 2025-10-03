
namespace gc.api.core.Entidades.Tipos
{
	public class TipoConciliado : EntidadBase
	{
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public string ct_tipo { get; set; } = string.Empty;
		public string ct_descripcion { get; set; } = string.Empty;
		public char ct_modo { get; set; }
		public char ct_concilia { get; set; }
	}
}
