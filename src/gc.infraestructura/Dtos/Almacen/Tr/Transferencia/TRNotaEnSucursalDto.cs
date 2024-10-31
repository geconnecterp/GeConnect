
namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRNotaEnSucursalDto : Dto
	{
        public string Titulo { get; set; } = string.Empty;
        public string Nota { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public int autorizacion { get; set; }
    }
}
