using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta
{
	public class ConsultaOPModel
	{
		public GridCoreSmart<OrdenDePagoConsultaDto> GrillaOP { get; set; }
		public decimal Importe { get; set; }
		public SelectList ListaTipoCertificado { get; set; }
		public bool MostrarListaTipoCertificado { get; set; } = false;
	}
}
