
namespace gc.infraestructura.Dtos.Financieros
{
	public class LiqDeEmpleadoListaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string le_compte { get; set; } = string.Empty;
		public string le_periodo { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string le_concepto { get; set; } = string.Empty;
		public DateTime le_fecha { get; set; }
		public char le_anulada { get; set; }
		public DateTime le_anulada_fecha { get; set; }
		private string _str_le_anulada;

		public string str_le_anulada
		{
			get { return le_anulada == 'S' ? "SI" : "NO"; }
			set { _str_le_anulada = value; }
		}
	}
}
