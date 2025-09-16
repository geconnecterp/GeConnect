
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoExtractoDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public string ext_concepto { get; set; } = string.Empty;
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
		public decimal ext_saldo { get; set; } = 0.00M;
		public char? ext_conciliado { get; set; }
		public string ext_conciliado_nro { get; set; } = string.Empty;
		public string ext_conciliado_tipo { get; set; } = string.Empty;
		public string usu_id_carga { get; set; } = string.Empty;
		public string usu_id_concilia { get; set; } = string.Empty;
		public string ct_tipo { get; set; } = string.Empty;
		public string ct_modo { get; set; } = string.Empty;
		public string ct_descripcion { get; set; } = string.Empty;
		public DateTime ban_cierre_extracto { get; set; }
		public char? ctl_cierre { get; set; }

		private bool _strConciliado;

		public bool strConciliado
		{
			get { return ext_conciliado != null && ext_conciliado == 'S'; }
			set { _strConciliado = value; }
		}

		private bool _strCierre;

		public bool strCierre
		{
			get { return ctl_cierre != null && ctl_cierre == 'S'; }
			set { _strCierre = value; }
		}

	}
}
