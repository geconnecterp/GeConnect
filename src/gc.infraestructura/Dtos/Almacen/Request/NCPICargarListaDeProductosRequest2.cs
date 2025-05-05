
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class NCPICargarListaDeProductos2Request : QueryFilters
	{
       // public string Tipo { get; set; } = string.Empty;
        public string Adm_Id { get; set; } = string.Empty;
		public string Usu_Id { get; set; } = string.Empty;
	}
}
