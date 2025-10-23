
namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class ProyFinanDto : Dto
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public int semana { get; set; }
		public decimal che_emi_nent { get; set; } = 0.00M;
		public decimal che_emi_ent { get; set; } = 0.00M;
		public decimal apagar { get; set; } = 0.00M;
		public decimal proy_gastos { get; set; } = 0.00M;
		public decimal proy_imp { get; set; } = 0.00M;
		public decimal che_cartera { get; set; } = 0.00M;
		public decimal che_depo { get; set; } = 0.00M;
		public decimal valores_alcobro { get; set; } = 0.00M;
		public decimal acobrar { get; set; } = 0.00M;
		public decimal valores_alcobro_v { get; set; } = 0.00M;
		public decimal proy_vtas { get; set; } = 0.00M;
		public decimal proy_efe_porc { get; set; } = 0.00M;
		public decimal plazo_fijo { get; set; } = 0.00M;
		public decimal valores_alcobro_ven { get; set; } = 0.00M;
		public decimal saldo_bco { get; set; } = 0.00M;
		public decimal saldo_bco_rojo { get; set; } = 0.00M;
		public decimal trans { get; set; } = 0.00M;
		public decimal acobrar_mes_ant { get; set; } = 0.00M;
		public string? leyendaSemana { get; set; }

		private decimal _cheque_emit_mas_trans_bco;

		public decimal cheque_emit_mas_trans_bco
		{
			get { return che_emi_ent + che_emi_nent + trans; }
			set { _cheque_emit_mas_trans_bco = value; }
		}

		private decimal _total_proy_egresos;

		public decimal total_proy_egresos
		{
			get { return che_emi_nent + che_emi_ent + apagar + proy_gastos + proy_imp; }
			set { _total_proy_egresos = value; }
		}

		private decimal _total_proy_ingresos;

		public decimal total_proy_ingresos
		{
			get { return che_cartera + che_depo + valores_alcobro + acobrar; }
			set { _total_proy_ingresos = value; }
		}

	}
}
