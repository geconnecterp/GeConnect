using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class AConsolidarConteosDto : Dto, IProductoConUnidad
	{
		public string or_compte { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public decimal p_pcosto { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public int unidad_pres { get; set; }
		public int bulto { get; set; }
		public decimal us { get; set; }
		public decimal cantidad { get; set; }
		public string vto { get; set; } = string.Empty;
		public int bultos_ctl { get; set; }
		public decimal us_ctl { get; set; }
		public decimal cantidad_ctl { get; set; }
		public decimal pcd_pedida { get; set; }
		public string estado { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}
}
