namespace gc.sitio.Areas.Financieros.Models
{
	public class DetalleDePresentacionModel
	{
		public string concepto { get; set; } = string.Empty;
		public DateTime fecha_acreditacion { get; set; }
		public string cuenta_en_cartera { get; set; } = string.Empty;
		public decimal saldo_cuenta_en_cartera { get; set; } = 0.00M;
		public decimal importe_a_presentar_en_cartera { get; set; } = 0.00M;
		public decimal saldo_a_constituir_en_cartera { get; set; } = 0.00M;
		public string cuenta_al_cobro { get; set; } = string.Empty;
		public decimal saldo_cuenta_al_cobro { get; set; } = 0.00M;
		public decimal importe_a_presentar_al_cobro { get; set; } = 0.00M;
		public decimal saldo_a_constituir_al_cobro { get; set; } = 0.00M;
		public string ctaf_id_cartera { get; set; } = string.Empty;
		public string ctaf_desc_cartera { get; set; } = string.Empty;
		public string ctaf_id_al_cobro { get; set; } = string.Empty;
		public string ctaf_desc_al_cobro { get; set; } = string.Empty;
	}
}
