
namespace gc.infraestructura.Dtos
{
    public class TipoTributoDto : Dto
    {
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string ins_tipo { get; set; } = string.Empty;
		public bool carga_aut_discriminado { get; set; }
		public bool carga_aut_no_discriminado { get; set; }
		public int orden { get; set; }
	}
}
