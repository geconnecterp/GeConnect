
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroTraRepoCtagDto : Dto
	{
		public string tra_compte { get; set; } = string.Empty;
		public string ttra_id { get; set; } = string.Empty;
		public string ttra_desc { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public DateTime tra_fecha { get; set; }
		public DateTime tra_fecha_movi { get; set; }
		public string tra_concepto { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public char tra_anulada { get; set; }
		public DateTime tra_anulada_fecha { get; set; }
		public int tra_item { get; set; }
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public string cm_motivo { get; set; } = string.Empty;
		public int grupo { get; set; }
		public decimal cm_importe { get; set; } = 0.00M;
		public decimal cm_iva { get; set; } = 0.00M;
		public decimal cm_percepciones { get; set; } = 0.00M;
		public decimal cm_total { get; set; } = 0.00M;
	}
}
