
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;

namespace gc.infraestructura.Dtos.ABM
{
	public class ABMMedioDePagoSearchDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		[Display(Name = "ID")]
		public string Ins_Id { get; set; } = string.Empty;
		[Display(Name = "Descripción")]
		public string Ins_Desc { get; set; } = string.Empty;
		public string Ins_Lista { get; set; } = string.Empty;
		//public string Mon_Codigo { get; set; } = string.Empty;
		public char Ins_Detalle { get; set; }
		//public string? Ins_Dato1_Desc { get; set; }
		//public string? Ins_Dato2_Desc { get; set; }
		//public string? Ins_Dato3_Desc { get; set; }
		[Display(Name = "Tipo")]
		public string Tcf_Id { get; set; } = string.Empty;
		public char Ins_Tiene_Vto { get; set; }
		public char Ins_Arqueo { get; set; }
		//public char Ins_Vuelto { get; set; }
		[Display(Name = "Activo")]
		public char Ins_Vigente { get; set; }
		public string? Ins_Razon_Social { get; set; }
		public string? Ins_Cuit { get; set; }
		public decimal Ins_Comision { get; set; } = 0.00M;
		public decimal Ins_Comision_Fija { get; set; } = 0.00M;
		public decimal Ins_Ret_Gan { get; set; }
		public decimal Ins_Ret_Ib { get; set; }
		public decimal Ins_Ret_Iva { get; set; }
		public string? Ctaf_Id_Link { get; set; }
		//public int? Ins_Dias_Acre { get; set; }
		//public string? Inse_Empresa { get; set; }
		//public string? Ins_Id_Barrado { get; set; }
		//public string? Ins_Id_Archivo { get; set; }
		public string? Ins_Id_Pos { get; set; }
		public string? Ins_Id_Pos_Ctls { get; set; }


	}

	public class MedioDePagoListaDto : Dto
	{
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string ins_lista { get; set; } = string.Empty;
		public string ins_dato1_desc { get; set; } = string.Empty;
		public string ins_dato2_desc { get; set; } = string.Empty;
		public string ins_dato3_desc { get; set; } = string.Empty;
		public char ins_detalle { get; set; }
		public decimal ins_comision { get; set; }
		public decimal ins_comision_fija { get; set; }
		public string ins_razon_social { get; set; } = string.Empty;
		public string ins_cuit { get; set; } = string.Empty;
		public decimal ins_ret_gan { get; set; }
		public decimal ins_ret_ib { get; set; }
		public decimal ins_ret_iva { get; set; }
		public char ins_arqueo { get; set; }
		public char ins_tiene_vto { get; set; }
		public char ins_vigente { get; set; }
		public int ctaf_id_link_check { get; set; }
		public string ctaf_id_link { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public string tcf_desc { get; set; } = string.Empty;
		public string ins_id_pos { get; set; } = string.Empty;
		public string ins_id_pos_ctls { get; set; } = string.Empty;


	}
}
