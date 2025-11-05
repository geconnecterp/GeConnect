using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class LiqDeEmpleadoModel
	{
		public SelectList Anio{ get; set; }
		public string SelectedValueAnio { get; set; } = string.Empty;
		public SelectList Mes { get; set; }
		public string SelectedValueMes { get; set; } = string.Empty;
		public string Concepto { get; set; } = string.Empty;
		public decimal PorcTope { get; set; } = 0.00M;
		public bool ActualizaTope { get; set; } = true;
		public GridCoreSmart<LiqEmpleadoEncabezadoDto> GrillaEncabezado { get; set; }
		public GridCoreSmart<LiqEmpleadoDetalleDto> GrillaDetalle { get; set; }
	}
}
