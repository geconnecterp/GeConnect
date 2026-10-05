
namespace gc.infraestructura.Dtos.Almacen.RemitoExterno
{
	public class RemitoExternoListaDto : Dto
	{
		public int Total_registros { get; set; }
		public int Total_paginas { get; set; }
		public string rem_compte { get; set; } = string.Empty;
		public string rem_nombre { get; set; } = string.Empty;
		public string rem_domicilio { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string rem_cuit { get; set; } = string.Empty;
		public DateTime rem_fecha { get; set; }
		public string rem_obs { get; set; } = string.Empty;
		public string rem_retirado_por { get; set; } = string.Empty;
		public char reme_id { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string pre_id { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string pv_compte { get; set; } = string.Empty;
	}
}
