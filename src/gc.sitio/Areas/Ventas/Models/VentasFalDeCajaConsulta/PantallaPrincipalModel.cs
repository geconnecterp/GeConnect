namespace gc.sitio.Areas.Ventas.Models.VentasFalDeCajaConsulta
{
	public class PantallaPrincipalModel
	{
		public string Sucursal { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
