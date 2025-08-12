
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

	public class FinancieroCarteraDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string fc_compte { get; set; } = string.Empty;
		public int fc_item { get; set; }
		public string? fc_dato1_valor { get; set; }
		public string? fc_dato2_valor { get; set; }
		public string? fc_dato3_valor { get; set; }
		public DateTime? fc_fecha_valor { get; set; }
		public decimal fc_importe { get; set; } = 0.00M;
		public DateTime? fc_fecha { get; set; }
		public char propio { get; set; }
		public string? cta_id { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string? ins_dato1_desc { get; set; }
		public string? ins_dato2_desc { get; set; }
		public string? ins_dato3_desc { get; set; }
		public string? fc_concepto { get; set; }
		public int? tra_conciliada_nro { get; set; }
		public string? ins_id { get; set; }
		public string tcf_id { get; set; } = string.Empty;
		public string concepto_valor { get; set; } = string.Empty;
	}
}
