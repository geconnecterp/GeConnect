
namespace gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP
{
	public class CompteJbiDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public DateTime cm_fecha_carga { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public decimal cm_total { get; set; }
		public char justificado { get; set; }
		public DateTime? fecha_just { get; set; }
		public string motivo_just { get; set; } = string.Empty;
		public string usu_id_bi { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;

		private bool _justificado_bool;

		public bool justificado_bool
		{
			get { return justificado == 'N' ? false : true; }
			set { _justificado_bool = value; }
		}
		private string _concepto { get; set; } = string.Empty;
		public string concepto
		{
			get { return $"{tco_desc} ({tco_id}) {cm_compte}"; }
			set { _concepto = value ?? string.Empty; }
		}
	}
}
