namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class AutorizacionPendienteDto : Dto
	{
		public string Rp { get; set; } = string.Empty;
		public string Cta_id { get; set; } = string.Empty;
		public string Usu_id { get; set; } = string.Empty;
		public DateTime? Fecha { get; set; }
		public string Nota { get; set; } = string.Empty;
		public string Rpe_id { get; set; } = string.Empty;
		public string Cta_denominacion { get; set; } = string.Empty;
		public string Rp_hidden { get; set; } = string.Empty;
	}

	public class RegistroResponseDto
	{
		public short Resultado { get; set; }
		public string Resultado_msj { get; set; } = string.Empty;
	}

	public class AutoComptesPendientesDto : AutorizacionPendienteDto
	{
		public string Rpe_desc { get; set; } = string.Empty;
		public string Tco_id { get; set; } = string.Empty;
		public string Tco_desc { get; set; } = string.Empty;
		public string Cm_compte { get; set; } = string.Empty;
		public DateTime Cm_fecha { get; set; }
		public decimal Cm_importe { get; set; } = decimal.Zero;
	}


}
