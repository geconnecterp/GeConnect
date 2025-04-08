
namespace gc.api.core.Entidades
{
    public class TipoTributo : EntidadBase
    {
		public string ins_id { get; set; }
		public string ins_desc { get; set; }
		public string ins_tipo { get; set; }
		public bool carga_aut_discriminado { get; set; }
		public bool carga_aut_no_discriminado { get; set; }
		public int orden { get; set; }
	}
}
