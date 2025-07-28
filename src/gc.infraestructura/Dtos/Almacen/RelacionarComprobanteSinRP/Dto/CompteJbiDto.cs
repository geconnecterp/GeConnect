
namespace gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP
{
	public class CompteJbiDto : Dto
	{
		public string tco_id { get; set; }
		public string cm_compte { get; set; }
		public string dia_movi { get; set; }
		public DateTime cm_fecha { get; set; }
		public DateTime cm_fecha_carga { get; set; }
		public string usu_id { get; set; }
		public decimal cm_total { get; set; }
		public char justificado { get; set; }
		public DateTime? fecha_just { get; set; }
		public string motivo_just { get; set; }
		public string usu_id_bi { get; set; }
		public string op_compte { get; set; }
	}
}
