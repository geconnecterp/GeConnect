
namespace gc.infraestructura.Dtos.Inventario
{
	public class ConfirmarModificacionDeConteoRequest
	{
		public string p_id { get; set; } = string.Empty;
		public string box_id { get; set; } = string.Empty;
		public string inv_nro { get; set; } = string.Empty;
		public decimal cantidad { get; set; }
		public int carga_nro { get; set; }
		public string usu_id { get; set; } = string.Empty;
	}
}
