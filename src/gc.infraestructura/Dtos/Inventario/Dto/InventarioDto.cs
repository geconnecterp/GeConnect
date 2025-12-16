
namespace gc.infraestructura.Dtos.Inventario
{
	public class InventarioDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public char? inve_id { get; set; }
		public char? invt_id { get; set; }
		public string inv_descripcion { get; set; } = string.Empty;
		public DateTime inv_apertura { get; set; }
		public DateTime inv_cierre { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string as_nro { get; set; } = string.Empty;
		public char inv_actu { get; set; }
		public DateTime? inv_ult_lectura_stk { get; set; }
	}

	public class  InventarioListaDto : InventarioDto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
	}
}
