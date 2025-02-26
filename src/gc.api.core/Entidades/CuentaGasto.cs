
namespace gc.api.core.Entidades
{
    public class CuentaGasto : EntidadBase
    {
		public string Ctag_Id { get; set; } = string.Empty;
		public string Ctag_Denominacion { get; set; } = string.Empty;
		public string Tcg_Id { get; set; } = string.Empty;
		public string Tcg_Desc { get; set; } = string.Empty;
		public bool Ctag_Ingreso { get; set; }
		public string Ctag_Valores_Anombre { get; set; } = string.Empty;
		public char Ctag_Activo { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
		public string Ccb_Desc { get; set; } = string.Empty;
	}
}
