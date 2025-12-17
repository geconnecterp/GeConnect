
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
		public string inve_desc { get; set; } = string.Empty;
		public string invt_desc { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;

	}

}
