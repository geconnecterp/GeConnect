
namespace gc.infraestructura.Dtos.Financieros
{
	public class RegistroExtractoDto : Dto
	{
		public int orden { get; set; }
		public string ctaf_id { get; set; }
		public DateTime ext_fecha { get; set; }
		public DateTime ext_fecha_movi { get; set; }
		public string extr_id { get; set; }
		public string extr_desc { get; set; }
		public string concepto { get; set; }
		public decimal importe { get; set; }
		public string ct_tipo { get; set; }
		public string conciliado { get; set; }
		public int? conciliado_nro { get; set; }
		public string a_conciliar { get; set; }
		public int a_conciliar_nro { get; set; }
		public string a_conciliar_tipo { get; set; }
		private bool _bool_conciliado;

		public bool bool_conciliado
		{
			get { return conciliado == "S"; }
			set { _bool_conciliado = value; }
		}
		private bool _bool_a_conciliar;

		public bool bool_a_conciliar
		{
			get { return a_conciliar == "S"; }
			set { _bool_a_conciliar = value; }
		}
		private bool _puedo_seleccionar;

		public bool puedo_seleccionar
		{
			get { return conciliado == "N" && (string.IsNullOrWhiteSpace(a_conciliar) || a_conciliar == "N"); }
			set { _puedo_seleccionar = value; }
		}
	}
}
