
namespace gc.infraestructura.Dtos.Financieros
{
	public class RegistroSistemaDto : Dto
	{
		public string ctaf_id { get; set; }
		public DateTime cf_fecha_concilia { get; set; }
		public string dia_movi { get; set; }
		public string cf_compte { get; set; }
		public int cf_item { get; set; }
		public string tco_id { get; set; }
		public string concepto { get; set; }
		public decimal importe { get; set; }
		public string ct_tipo { get; set; }
		public string conciliado { get; set; }
		public int? conciliado_nro { get; set; }
		public string a_cociliar { get; set; }
		public int a_cociliar_nro { get; set; }
		public string a_cociliar_tipo { get; set; }

		private bool _bool_conciliado;

		public bool bool_conciliado
		{
			get { return conciliado == "S"; }
			set { _bool_conciliado = value; }
		}
		private bool _bool_a_conciliar;

		public bool bool_a_conciliar
		{
			get { return a_cociliar == "S"; }
			set { _bool_a_conciliar = value; }
		}
		private bool _puedo_seleccionar;

		public bool puedo_seleccionar
		{
			get { return conciliado == "N" && (string.IsNullOrWhiteSpace(a_cociliar) || a_cociliar == "N"); }
			set { _puedo_seleccionar = value; }
		}
	}
}
