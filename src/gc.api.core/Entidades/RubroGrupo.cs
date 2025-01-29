
namespace gc.api.core.Entidades
{
	public class RubroGrupo : EntidadBase
	{
        public string rubg_id { get; set; } = string.Empty;
        public string rubg_desc { get; set; } = string.Empty;
		public string rubg_lista { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public char rubg_actu { get; set; } = char.MinValue;
    }
}
