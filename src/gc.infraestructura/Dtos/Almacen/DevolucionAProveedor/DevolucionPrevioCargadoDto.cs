﻿
namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	public class DevolucionPrevioCargadoDto : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public DateTime fecha_carga { get; set; }
		public string at_id { get; set; } = string.Empty;
		public string at_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public int bulto { get; set; }
		public decimal us { get; set; } = 0.000M;
		public decimal cantidad { get; set; } = 0.000M;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal ps_bulto { get; set; } = 0.000M;
		public DateTime vto { get; set; }
		public string up_id { get; set; } = string.Empty;
	}
}
