
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAutPIDto : Dto
	{
		public string pi_compte { get; set; } = string.Empty;
		public DateTime pi_fecha { get; set; }
		public string pie_id { get; set; } = string.Empty;
		public string pie_desc { get; set; } = string.Empty;
		public string pit_id { get; set; } = string.Empty;
		public string pit_desc { get; set; } = string.Empty;
		public string pi_nota { get; set; } = string.Empty;
        public string adm_id { get; set; }=string.Empty;
		public string adm_desc { get; set; } = string.Empty;
    }
}
