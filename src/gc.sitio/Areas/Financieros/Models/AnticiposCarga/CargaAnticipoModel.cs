using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CargaAnticipoModel
	{
		public int cuotasModalCarga { get; set; }
		public decimal importeModalCarga { get; set; }
		public decimal interesModalCarga { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_desc { get; set; } = string.Empty;
		public GridCoreSmart<CuentaDto> CuentaEmpleados { get; set; }
		public GridCoreSmart<CuentaDto> CuentaEmpleadosParaCargar { get; set; }
	}
}
