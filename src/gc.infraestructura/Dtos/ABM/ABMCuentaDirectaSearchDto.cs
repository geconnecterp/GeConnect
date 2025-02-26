
namespace gc.infraestructura.Dtos.ABM
{
	public class ABMCuentaDirectaSearchDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string Ctag_Id { get; set; } = string.Empty;
		public string Ctag_Denominacion { get; set; } = string.Empty;
		public string Tcg_Id { get; set; } = string.Empty;
		public string Tcg_Desc { get; set; } = string.Empty;
		public bool Ctag_Ingreso { get; set; }
		public string Ctag_Valores_Anombre { get; set; } = string.Empty;
		public char Ctag_Activo { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
	}
}
