
namespace gc.infraestructura.Dtos.OrdenDePago.Dtos
{
	public class OPDebitoYCreditoDelProveedorDto : Dto
	{
		public string cta_id { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public int cm_compte_cuota { get; set; } = 0;
		public DateTime cv_fecha_vto { get; set; }
		public decimal cv_importe { get; set; } = 0.00M;
		public decimal cv_imputado { get; set; } = 0.00M;
		public decimal cv_importe_ori { get; set; } = 0.00M;
		public string? cv_estado { get; set; }
		public DateTime cv_fecha_carga { get; set; }
		public string cv_concepto { get; set; } = string.Empty;
		public string? ve_id { get; set; }
		public string? ccb_id { get; set; }
		public string concepto { get; set; } = string.Empty;
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_motivo { get; set; } = string.Empty;
	}
}
