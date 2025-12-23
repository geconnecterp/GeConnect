using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InventarioCargaGrillaUsuariosModel
	{
		public GridCoreSmart<UsuarioEnInventarioDto> GrillaUsuarios { get; set; }
	}
}
