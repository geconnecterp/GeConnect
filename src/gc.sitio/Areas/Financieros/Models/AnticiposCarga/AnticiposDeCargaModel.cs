using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CargaDeAnticiposModel
	{
		public SelectList ListaTipo { get; set; }
		public string selectedValue { get; set; } = string.Empty;
		public string Concepto { get; set; }
		public decimal porc_interes { get; set; }
		public string prov_id_selected { get; set; } = string.Empty;
		public string prov_denominacion_selected { get; set; } = string.Empty;
		public GridCoreSmart<AnticipoDto> GrillaAnticipos { get; set; }
	}
}
