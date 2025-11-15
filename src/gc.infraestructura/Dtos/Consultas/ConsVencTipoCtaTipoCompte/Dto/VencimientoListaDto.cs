
namespace gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte	
{
	public class VencimientoListaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public decimal total_importe { get; set; } = 0.00M;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_list { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string comprobante { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public int cm_compte_cuota { get; set; }
		public DateTime cv_fecha_vto { get; set; }
		public DateTime cv_fecha_carga { get; set; }
		public int atraso { get; set; }
		public decimal cv_importe { get; set; } = 0.00M;
		public decimal cv_importe_ori { get; set; } = 0.00M;
		public string cv_concepto { get; set; } = string.Empty;
		public string cv_estado { get; set; } = string.Empty;
		public string ccb_id { get; set; } = string.Empty;

	}
}
