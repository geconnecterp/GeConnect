
namespace gc.infraestructura.Dtos
{
	public class TipoContactoDto : Dto
	{
        public string tc_id { get; set; } = string.Empty;
        public string tc_desc { get; set; } = string.Empty;
		public string tc_cliente { get; set; } = string.Empty;
		public string tc_proveedor { get; set; } = string.Empty;
	}
}
