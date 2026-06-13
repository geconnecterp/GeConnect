
namespace gc.infraestructura.Dtos
{
	public class CajaProcesoListaDto : Dto
	{
		public int total_registros { get; set; } = 0;
		public int total_paginas { get; set; } = 0;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public DateTime caja_habilitacion { get; set; }
		public DateTime caja_cierre_grl { get; set; }
		public int caja_nro_cierres { get; set; }
	}
}
