
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRObtenerListaDto : Dto
	{
		public string ti { get; set; } = string.Empty;
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_nombre_gen { get; set; } = string.Empty;
		public string adm_id_des { get; set; } = string.Empty;
		public string adm_nombre_des { get; set; } = string.Empty;
		public DateTime fecha { get; set; }
		public string nota { get; set; } = string.Empty;
		public string tie_id { get; set; } = string.Empty;
		public string tie_desc { get; set; } = string.Empty;
		public string tit_id { get; set; } = string.Empty;
		public string tit_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string pi_compte { get; set; } = string.Empty;
		public string re_compte { get; set; } = string.Empty;
		public string pv_compte { get; set; } = string.Empty;
	}
}
