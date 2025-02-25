
namespace gc.api.core.Entidades
{
    public class Banco : EntidadBase
	{
		public string Ctaf_Id { get; set; } = string.Empty;
		public string Ban_Razon_Social { get; set; } = string.Empty;
		public string Ban_Cuit { get; set; } = string.Empty;
		public char Tcb_Id { get; set; }
		public string Tcb_Desc { get; set; } = string.Empty;
		public string? Ban_Cuenta_Nro { get; set; }
		public string? Ban_Cuenta_Cbu { get; set; }
		public char Mon_Codigo { get; set; }
		public string Mon_Desc { get; set; } = string.Empty;
		public int? Ban_Che_Nro { get; set; }
		public int? Ban_Che_Desde { get; set; }
		public int? Ban_Che_Hasta { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
		public string Ccb_Desc { get; set; } = string.Empty;
		public string? Ccb_Id_Diferido { get; set; }
		public string Ccb_Desc_Diferido { get; set; } = string.Empty;
		public string? Ctag_Id { get; set; }
		public string Ctag_Denominacion { get; set; } = string.Empty;
	}
}
