using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.ABM.Request
{
	public class ABMClienteRequest : CuentaABMDto
	{
		public string destinoDeOperacion { get; set; } = string.Empty;
		public string tipoDeOperacion { get; set; } = string.Empty;
		public string jsonString { get; set; } = string.Empty;
    }
}
