
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAutDepoDto:Dto
	{
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public bool seleccionado { get; set; } = false;
	}
}
