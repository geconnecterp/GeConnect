
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRRemitoDto : Dto
	{
		public string re_compte { get; set; } = string.Empty;
		public DateTime re_fecha_gen { get; set; }
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_nombre_gen { get; set; } = string.Empty;
		public string usu_id_gen { get; set; } = string.Empty;
		public string usu_apellidoynombre_gen { get; set; } = string.Empty;
		public DateTime re_fecha_des { get; set; }
		public string adm_id_des { get; set; } = string.Empty;
		public string adm_nombre_des { get; set; } = string.Empty;
		public string usu_id_des { get; set; } = string.Empty;
		public string usu_apellidoynombre_des { get; set; } = string.Empty;
		public string ree_id { get; set; } = string.Empty;
		public string ree_desc { get; set; } = string.Empty;
		public string re_ajuste { get; set; } = string.Empty;
		public string as_compte { get; set; } = string.Empty;
		public string pv_compte { get; set; } = string.Empty;
		public int red_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public int red_bulto_gen { get; set; }
		public decimal red_cantidad_gen { get; set; }
		public int red_bulto_des { get; set; }
		public decimal red_cantidad_des { get; set; }
		public decimal p_pcosto_repo { get; set; }
	}
}
