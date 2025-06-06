
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaDtosListaDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public int item { get; set; }
		public decimal dto { get; set; } = 0.00M;
		public decimal dto_importe { get; set; } = 0.00M;
		public char dto_fijo { get; set; } = 'N';
		public char dto_sobre_total { get; set; } = 'N';
		public char dtoc_id { get; set; }
		public string dto_obs { get; set; } = string.Empty;
		public string dtoc_desc { get; set; } = string.Empty;

		private bool _dto_fijo_bool;

		public bool dto_fijo_bool
		{
			get { return dto_fijo == 'N' ? false : true; }
			set { _dto_fijo_bool = value; }
		}
		private bool _dto_sobre_total_bool;

		public bool dto_sobre_total_bool
		{
			get { return dto_sobre_total == 'N' ? false : true; }
			set { _dto_sobre_total_bool = value; }
		}

	}
}
