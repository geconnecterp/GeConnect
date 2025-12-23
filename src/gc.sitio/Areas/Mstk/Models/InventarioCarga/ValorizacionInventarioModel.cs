using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ValorizacionInventarioModel
	{
		public bool EsTipoBox { get; set; }
		public GridCoreSmart<RubroEnInventarioDto> GrillaInvRubros { get; set; }
		public GridCoreSmart<InventarioBoxDto> GrillaInvBoxes { get; set; }
		public string inv_nro { get; set; } = string.Empty;
		public string inv_descripcion { get; set; } = string.Empty;
	}
}
