
namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class RPRxULDetalleDto : Dto
	{
		public string id { get; set; } = string.Empty;
		public string ul_id { get; set; } = string.Empty;
        public int item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
        public string up_desc { get; set; } = string.Empty;
        public int unidad_pres { get; set; }
        public int bulto { get; set; }
		public decimal us { get; set; } = 0.000M;
		public decimal cantidad { get; set; } = 0.000M;
		public string vto { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string p_con_vto { get; set; } = string.Empty;
        public int p_con_vto_min { get; set; }
        public DateTime p_con_vto_ctl { get; set; }
    }

	public class RTRxULDetalleDto : RPRxULDetalleDto
	{ }
}
