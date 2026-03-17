
namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class CambioDePrecioDto : Dto
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal pcd_pvta { get; set; }
		public decimal p_pvta { get; set; }
		public decimal p_pvta_oferta { get; set; }
		public decimal p_vta_ctl { get; set; }
		public char selecciona { get; set; }
		public bool SeleccionaItem => selecciona == 'S';
	}
}
