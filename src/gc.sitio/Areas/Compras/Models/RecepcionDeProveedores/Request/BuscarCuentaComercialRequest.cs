namespace gc.sitio.Areas.Compras.Models.RecepcionDeProveedores.Request
{
	public class BuscarCuentaComercialRequest
	{
		public string cuenta { get; set; } = string.Empty;
		public char tipo { get; set; }
		public string vista { get; set; } = string.Empty;
	}
}
