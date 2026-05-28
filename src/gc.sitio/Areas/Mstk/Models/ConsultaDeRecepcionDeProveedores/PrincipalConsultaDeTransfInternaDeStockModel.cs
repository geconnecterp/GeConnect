namespace gc.sitio.Areas.Mstk.Models.ConsultaDeRecepcionDeProveedores
{
	public class PrincipalConsultaDeTransfInternaDeStockModel
	{
		public string Sucursales { get; set; } = string.Empty;
		public string Proveedores { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
