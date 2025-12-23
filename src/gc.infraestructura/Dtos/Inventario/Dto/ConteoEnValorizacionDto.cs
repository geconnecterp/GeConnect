
namespace gc.infraestructura.Dtos
{
	public class ConteoEnValorizacionDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public int carga_nro { get; set; }
		public string carga_des { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public int inv_grupo { get; set; }
		public int invd_unidad_pres { get; set; }
		public int invd_bulto { get; set; }
		public decimal invd_unidad_suelta { get; set; } = 0.000M;
		public decimal invd_cantidad { get; set; } = 0.000M;
	}
}
