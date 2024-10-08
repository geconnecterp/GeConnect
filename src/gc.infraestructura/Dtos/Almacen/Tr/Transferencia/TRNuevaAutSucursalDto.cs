
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRNuevaAutSucursalDto : Dto
	{
		public int orden { get; set; } = 0;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public int pallet_aprox { get; set; } = 0;
		public string nota { get; set; } = string.Empty;
		public int aut_a_generar { get; set; } = 0;
    }
}
