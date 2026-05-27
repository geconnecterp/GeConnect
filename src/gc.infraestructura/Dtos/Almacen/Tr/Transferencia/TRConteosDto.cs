
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRConteosDto : Dto
	{
		public string ti { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal pedido { get; set; }
		public decimal cantidad { get; set; }
		public int bulto { get; set; }
		public decimal us { get; set; }
		public int unidad_pres { get; set; }
		public DateTime vto { get; set; }
	}
}
