
namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class RprCtlItemDetalleDto : Dto, IProductoConUnidad
	{
		public string rp_compte { get; set; } = string.Empty;
		public int rpd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal in_alicuota { get; set; }
		public decimal iva_alicuota { get; set; }
		public string iva_situacion { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string oc_compte { get; set; } = string.Empty;
		public int rpd_unidad_pres { get; set; }
		public int rpd_bulto_recibidos { get; set; }
		public decimal rpd_unidad_suelta { get; set; }
		public decimal rpd_cantidad { get; set; }
		public decimal rpd_cantidad_ori { get; set; }
		public int rpd_bulto_compte { get; set; }
		public decimal rpd_unidad_suelta_compte { get; set; }
		public decimal rpd_cantidad_compte { get; set; }
		public bool PermiteDecimales => up_tipo == "P";
	}
}
