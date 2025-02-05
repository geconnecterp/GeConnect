using gc.infraestructura.Dtos.ABM;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class MedioDePagoAbmModel
	{
		public MedioDePagoABMDto MedioDePago { get; set; }
		public SelectList ComboMoneda { get; set; }
		public SelectList ComboFinanciero { get; set; }
	}
}
