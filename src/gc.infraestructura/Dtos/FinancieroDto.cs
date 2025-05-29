
namespace gc.infraestructura.Dtos
{
	public class FinancieroDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string ctaf_lista { get; set; } = string.Empty;
		public string ctaf_activo { get; set; } = string.Empty;
	}

	public class FinancieroListaDto : FinancieroDto
	{
		public string Ctaf_Estado { get; set; } = string.Empty;
		public string Ctaf_Estado_Des { get; set; } = string.Empty;
		public decimal? Ctaf_Saldo { get; set; } = 0.000M;
		public string Adm_Id { get; set; } = string.Empty;
		public string Tcf_Id { get; set; } = string.Empty;
		public string Tcf_Desc { get; set; } = string.Empty;
		public string? Ins_Id { get; set; }
		public string? Ins_Desc { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
		public string? Ccb_Id_Diferido { get; set; }
		public string? Ctag_Id { get; set; }
		public string? Mon_Codigo { get; set; }
		public string? Cta_Id { get; set; }
	}

	public class FinancieroDesdeSeleccionDeTipoDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public decimal ctaf_saldo { get; set; } = 0.00M;
		public string ins_id { get; set; } = string.Empty;
		public string ban_razon_social { get; set; } = string.Empty;
		public string ban_cuenta_nro { get; set; } = string.Empty;
		public int? ban_che_desde { get; set; }
		public int? ban_che_hasta { get; set; }
		public int? ban_che_nro { get; set; }
		public string mon_codigo { get; set; } = string.Empty;
		public string tcb_id { get; set; } = string.Empty;
	}
}
