namespace gc.sitio.Areas.Mstk.Models.ConsultaDeAjusteDeStock
{
	public class PrincipalConsultaDeAjusteDeStockModel
	{
		public string Sucursales { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
