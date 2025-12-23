
namespace gc.infraestructura.Dtos
{
	public class UsuarioEnInventarioDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public string inv_descripcion { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string inv_grupo { get; set; } = string.Empty;
	}
}
