
namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class RPRxULDto : Dto
	{
		public string ul_id { get; set; } = string.Empty;
		public string ule_id { get; set; } = string.Empty;
		public string ule_desc { get; set; } = string.Empty;
        public DateTime ul_fecha { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
		public string id { get; set; } = string.Empty;
    }

	public class RTRxULDto : RPRxULDto
	{ }
}
