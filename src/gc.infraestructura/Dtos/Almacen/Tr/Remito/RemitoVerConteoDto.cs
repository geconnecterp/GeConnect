
namespace gc.infraestructura.Dtos.Almacen.Tr.Remito
{
	public  class RemitoVerConteoDto : Dto
	{
		public string re_compte { get; set; } = string.Empty;
		public int item { get; set; } = 0;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_barrado { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public decimal enviado { get; set; } = 0.000M;
		public decimal recibido { get; set; } = 0.000M;
        public decimal diferencia { get; set; } = 0.000M;
		public string Row_color { get; set; } = "#ffffff";
	}
}
