
namespace gc.infraestructura.Dtos
{
	public class ABMChequeSearchDto : ABMChequeListaDto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
	}

	public class ABMChequeListaDto : Dto
	{
		public string bc_id { get; set; } = string.Empty;
		public string bc_denominacion { get; set; } = string.Empty;
		public string bc_lista { get; set; } = string.Empty;
		public string bc_plaza { get; set; } = string.Empty;
	}
}
