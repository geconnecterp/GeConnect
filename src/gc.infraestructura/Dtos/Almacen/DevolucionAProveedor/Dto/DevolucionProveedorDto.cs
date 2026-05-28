
namespace gc.infraestructura.Dtos.Almacen.DevolucionAProveedor
{
	public class DevolucionProveedorDto : Dto
	{
		public string dv_compte { get; set; } = string.Empty;
		public DateTime dv_fecha { get; set; }
		public string dv_motivo { get; set; } = string.Empty;
		public decimal dv_importe { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string pv_compte { get; set; } = string.Empty;
	}

	public class DevolucionProveedoresListaDto : DevolucionProveedorDto
	{
		public int Total_registros { get; set; }
		public int Total_paginas { get; set; }
	}
}

