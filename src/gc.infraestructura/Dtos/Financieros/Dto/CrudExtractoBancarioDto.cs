
namespace gc.infraestructura.Dtos.Financieros
{
	public class CrudExtractoBancarioDto : Dto
	{
		public int orden { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public DateTime? ext_fecha_movi { get; set; }
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public string ext_concepto { get; set; } = string.Empty;
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
		public decimal ext_saldo { get; set; } = 0.00M;
		public string ct_tipo { get; set; } = string.Empty;
		public string ct_modo { get; set; } = string.Empty;
		public char ext_conciliado { get; set; }
		public int? ext_conciliado_nro { get; set; }
		public string? ext_conciliado_tipo { get; set; }
		public char ctl_cierre { get; set; }
		public string abm { get; set; } = string.Empty;
		private bool _ext_conciliado_bool;

		public bool ext_conciliado_bool
		{
			get { return ext_conciliado == 'S'; }
			set { _ext_conciliado_bool = value; }
		}
		private bool _ctl_cierre_bool;

		public bool ctl_cierre_bool
		{
			get { return ctl_cierre == 'S'; }
			set { _ctl_cierre_bool = value; }
		}

		public string? usu_id { get; set; }
		public string? usu_id_concilia { get; set; }
		public string? ct_descripcion { get; set; }
		public string? ban_cierre_extracto { get; set; }
		public string? ctaf_denominacion { get; set; }
		public int? resultado { get; set; }
		public string? resultado_msj { get; set; }
	}
}
