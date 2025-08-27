
namespace gc.infraestructura.Dtos
{
	public class FinancieroChequeDepositadoDto : Dto
	{
		public string tra_compte { get; set; } = string.Empty;
		public int tra_item { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string fc_dia_movi { get; set; } = string.Empty;
		public string fc_compte { get; set; } = string.Empty;
		public int fc_item { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string fc_dato1_valor { get; set; } = string.Empty;
		public string fc_dato2_valor { get; set; } = string.Empty;
		public string fc_dato3_valor { get; set; } = string.Empty;
		public DateTime? fc_fecha_valor { get; set; }
		public decimal fc_importe { get; set; } = 0.00M;
		public decimal fc_importe_ori { get; set; } = 0.00M;
		public DateTime? fc_fecha { get; set; }
		public char? fc_propio { get; set; }
		public string fc_cta_id { get; set; } = string.Empty;
		public string fc_concepto { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string caja_id { get; set; } = string.Empty;
		public char? tra_anu { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public DateTime tra_fecha_movi { get; set; }
		public bool rechazado { get; set; }
		public bool conciliado { get; set; }

		private string strRechazado;

		public string StrRechazado
		{
			get { return rechazado ? "SI" : "NO"; }
			set { strRechazado = value; }
		}

		private string strConciliado;

		public string StrConciliado
		{
			get { return conciliado ? "SI" : "NO"; }
			set { strConciliado = value; }
		}

		private string cliente;

		public string Cliente
		{
			get 
			{

				return string.IsNullOrEmpty(cta_denominacion) ? "" : $"{cta_denominacion} ({cta_id})"; 
			}
			set { cliente = value; }
		}

	}
}
