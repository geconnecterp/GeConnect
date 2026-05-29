
namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class AjusteDeStockListaDto : Dto
	{
		public int Total_registros { get; set; }
		public int Total_paginas { get; set; }
		public string as_compte { get; set; } = string.Empty;
		public DateTime as_fecha { get; set; }
		public string as_motivo { get; set; } = string.Empty;
		public string at_id { get; set; } = string.Empty;
		public string at_desc { get; set; } = string.Empty;
		public string ae_id { get; set; } = string.Empty;
		public string ae_desc { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string as_compte_ori { get; set; } = string.Empty;
	}
}
