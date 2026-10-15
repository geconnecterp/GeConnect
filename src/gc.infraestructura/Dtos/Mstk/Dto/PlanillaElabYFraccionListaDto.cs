
namespace gc.infraestructura.Dtos.Mstk
{
	public class PlanillaElabYFraccionListaDto : Dto
	{
		public int Total_registros { get; set; }
		public int Total_paginas { get; set; }
		public string ep_compte { get; set; } = string.Empty;
		public DateTime ep_fecha { get; set; }
		public string ep_motivo { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
	}
}
