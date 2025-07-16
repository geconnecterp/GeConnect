
namespace gc.infraestructura.Dtos.Almacen
{
	public class OrdenDePagoConsultaDto : OrdenDePagoListDto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
	}

	public class OrdenDePagoListDto : Dto
	{
		public string op_compte { get; set; } = string.Empty;
		public string opt_id { get; set; } = string.Empty;
		public string opt_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public decimal op_importe { get; set; } = 0.00M;
		public DateTime op_fecha { get; set; }
		public string op_concepto { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public char op_anulada { get; set; }
		public char op_impreso { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public bool certificado_ib { get; set; }
		public bool certificado_ga { get; set; }
		public bool certificado_iva { get; set; }
	}
}
