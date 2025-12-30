
namespace gc.infraestructura.Dtos
{
	public class InvRepoConteosPorUsuDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public int carga_nro { get; set; }
		public string carga_des { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public int p_unidad_pres { get; set; }
		public int invd_bulto { get; set; }
		public decimal invd_unidad_suelta { get; set; }
		public decimal invd_cantidad { get; set; }
		public char up_tipo { get; set; }
	}
}
