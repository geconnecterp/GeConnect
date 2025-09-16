
namespace gc.infraestructura.Dtos.Financieros
{
	public class MovimientoFinancieroListaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string tra_compte { get; set; } = string.Empty;
		public string ttra_id { get; set; } = string.Empty;
		public string ttra_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string tra_concepto { get; set; } = string.Empty;
		public DateTime tra_fecha { get; set; }
		public char tra_anulada { get; set; }
		public DateTime? tra_anulada_fecha { get; set; }
		public decimal tra_importe { get; set; } = 0.00M;

		private string _strAnulada;

		public string strAnulada
		{
			get { return tra_anulada == 'S' ? "Si" : "No"; }
			set { _strAnulada = value; }
		}

	}
}
