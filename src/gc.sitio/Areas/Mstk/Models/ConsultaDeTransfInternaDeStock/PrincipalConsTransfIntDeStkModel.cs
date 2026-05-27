namespace gc.sitio.Areas.Mstk.Models.ConsultaDeTransfInternaDeStock
{
	public class PrincipalConsTransfIntDeStkModel
	{
		public string SucursalesEnv { get; set; } = string.Empty;
		public string SucursalesRec { get; set; } = string.Empty;
		public string Tipos { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
