using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ProveedorABMFliaGrupoModel
	{
        public ProveedorGrupoModel ProveedorGrupo { get; set; }
        public GridCore<ProveedorGrupoDto> ListaProveedorGrupo { get; set; }
		public ProveedorABMFliaGrupoModel()
		{
			ProveedorGrupo = new ProveedorGrupoModel();
		}
	}
}
