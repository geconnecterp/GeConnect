
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAutSucursalesDto : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string orden { get; set; } = string.Empty;
		public bool tiene_ordenes { get; set; } = false;
	}
}
