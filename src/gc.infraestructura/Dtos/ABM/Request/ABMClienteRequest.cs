using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos.ABM.Request
{
	public class ABMClienteRequest : CuentaABMDto
	{
		public string AbmObject { get; set; } = string.Empty;
		public string AbmAction { get; set; } = string.Empty;
    }
}
