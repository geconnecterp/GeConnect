
namespace gc.infraestructura.Dtos.ABM
{
    public class ABMBancoSearchDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string Ctaf_Id { get; set; } = string.Empty;
		public string Ban_Razon_Social { get; set; } = string.Empty;
		public string Ban_Cuit { get; set; } = string.Empty;
		public string Tcb_Id { get; set; } = string.Empty;
		public string Tcb_Desc { get; set; } = string.Empty;
		public string Ban_Cuenta_Nro { get; set; } = string.Empty;
		public string? Ban_Cuenta_Cbu { get; set; }
		public string Mon_Codigo { get; set; } = string.Empty;
		public int? Ban_Che_Nro { get; set; }
		public int? Ban_Che_Desde { get; set; }
		public int? Ban_Che_Hasta { get; set; }
	}
}
