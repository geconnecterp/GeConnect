
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRVerConteosDto : Dto
	{
		public string ti { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public decimal p_pcosto { get; set; } = 0.000M;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string adm_min_excluye { get; set; } = string.Empty;
		public string adm_may_excluye { get; set; } = string.Empty;
		public int unidad_pres { get; set; } = int.MinValue;
		public int bulto { get; set; } = int.MinValue;
		public decimal us { get; set; } = 0.000M;
		public decimal cantidad { get; set; } = 0.000M;
		public DateTime vto { get; set; }
		public int bultos_c { get; set; } = int.MinValue;
		public decimal us_c { get; set; } = 0.000M;
		public decimal cantidad_c { get; set; } = 0.000M;
		public decimal cantidad_pi { get; set; } = 0.000M;
		public string concepto { get; set; } = string.Empty;

		private decimal diferencia;

		public decimal Diferencia
		{
			get { return cantidad - cantidad_c; }
			set { diferencia = value; }
		}
		private string? row_color;
		public string Row_color
		{
			get
			{
				if (diferencia != 0) return "#fc3b12";
				if (diferencia == 0 && cantidad > 0) return "#0dc556";
				if (diferencia == 0 && cantidad == 0) return "#e5e219";
				return "";
			}
			set { row_color = value; }
		}

		private int myVar;

		public int MyProperty
		{
			get { return myVar; }
			set { myVar = value; }
		}

	}
}
