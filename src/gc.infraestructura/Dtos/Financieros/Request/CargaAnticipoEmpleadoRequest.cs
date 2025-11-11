
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros
{
	public class CargaAnticipoEmpleadoRequest : RequestBase
	{
		public string ant_id { get; set; } = string.Empty;
		public string an_concepto { get; set; } = string.Empty;
		public decimal an_porc_interes { get; set; } = 0.00M;
		public string cta_id { get; set; } = string.Empty; //(cuenta de proveedor “contrapartida”)
		public string json_anticipos { get; set; } = string.Empty;
	}
}
