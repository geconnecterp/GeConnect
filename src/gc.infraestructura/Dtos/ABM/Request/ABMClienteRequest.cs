using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.ABM.Request
{
	public class ABMClienteRequest : CuentaABMDto
	{
		public string destinoDeOperacion { get; set; } = string.Empty;
		public char tipoDeOperacion { get; set; }
		public string jsonString { get; set; } = string.Empty;
    }
}
