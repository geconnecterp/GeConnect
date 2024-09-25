
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRPendienteDto : Dto
	{
        public string ti { get; set; }=string.Empty;
        public string adm_id_des { get; set; }=string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string nota { get; set; } = string.Empty;
        public string tie_id { get; set; } = string.Empty;
        public string tie_desc { get; set; } = string.Empty;
        public string pi_compte { get; set; } = string.Empty;
    }
}
