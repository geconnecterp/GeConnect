using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class AConsolidarPedidoClienteDetalleDto : Dto, IProductoConUnidad
	{
		public string or_compte { get; set; }
		public string pc_compte { get; set; }
		public string pcd_item { get; set; }
		public string p_id { get; set; }
		public string p_desc { get; set; }
		public string p_id_prov { get; set; }
		public decimal p_pcosto { get; set; }
		public string cta_id { get; set; }
		public string cta_denominacion { get; set; }
		public string up_id { get; set; }
		public string up_tipo { get; set; }
		public string up_desc { get; set; }
		public int unidad_pres { get; set; }
		public decimal cantidad { get; set; }
		public decimal pcd_pedida { get; set; }
		public string vto { get; set; }
		public char pcd_origen { get; set; }
		public string p_id_remplazo { get; set; }
		public string rub_id { get; set; }
		public string rub_desc { get; set; }
		public char reviso { get; set; }
		public bool EstaRevisado => char.ToUpper(reviso) == 'S';
		public bool PermiteDecimales => up_tipo == "P";
	}
}
