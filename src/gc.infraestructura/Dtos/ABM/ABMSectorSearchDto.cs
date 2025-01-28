using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.ABM
{
	public class ABMSectorSearchDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		[Display(Name = "ID")]
		public string sec_id { get; set; } = string.Empty;
		[Display(Name = "Descripción")]
		public string sec_desc { get; set; } = string.Empty;
		[Display(Name = "Lista")]
		public string sec_lista { get; set; } = string.Empty;
		public char sec_prefactura { get; set; } = char.MinValue;
	}
}
