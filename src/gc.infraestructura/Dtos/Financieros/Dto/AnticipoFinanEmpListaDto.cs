
namespace gc.infraestructura.Dtos.Financieros
{
	public class AnticipoFinanEmpListaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string an_compte { get; set; } = string.Empty;
		public string ant_id { get; set; } = string.Empty;
		public string ant_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string an_concepto { get; set; } = string.Empty;
		public DateTime an_fecha { get; set; }
		public char an_anulada { get; set; }
		public DateTime? an_anulada_fecha { get; set; }
		private string _str_an_anulada;

		public string str_an_anulada
		{
			get { return an_anulada == 'S' ? "SI" : "NO"; }
			set { _str_an_anulada = value; }
		}

	}
}
