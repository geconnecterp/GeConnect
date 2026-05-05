using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Consultas.Models
{
	public class AnalisisDeVentasMesModel
	{
		public GridCoreSmart<AnaVtaMesDto> ListaAnaVtaMes { get; set; }
	}
}
